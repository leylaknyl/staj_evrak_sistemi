
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using StajEvrakSistemi.Data;
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Controllers;

[Authorize(Roles = "Admin")]
public class AdminVeriController : Controller
{
    private readonly StajDbContext _db;

    public AdminVeriController(StajDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var ogrenciler = await _db.OgrenciKayitlari
            .AsNoTracking()
            .OrderBy(x => x.OgrenciNo)
            .ToListAsync();

        return View(ogrenciler);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> Yukle(IFormFile? dosya)
    {
        if (dosya == null ||
            dosya.Length == 0 ||
            dosya.Length > 2 * 1024 * 1024 ||
            Path.GetExtension(dosya.FileName).ToLowerInvariant() != ".csv")
        {
            TempData["Hata"] = "En fazla 2 MB boyutunda CSV yükleyin.";
            return RedirectToAction(nameof(Index));
        }

        string[] beklenenBasliklar =
        {
            "OgrenciNo", "AdSoyad", "Eposta", "Telefon",
            "Bolum", "KimlikNo", "Uyruk", "DogumTarihi",
            "Adres", "Sinif", "OncekiStajGunu",
            "SaglikYardimiAliyor"
        };

        var yeniKayitlar = new List<OgrenciKaydi>();

        var numaralar = (await _db.OgrenciKayitlari
            .Select(x => x.OgrenciNo)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        int atlanan = 0;
        int satir = 1;

        try
        {
            using var okuyucu = new StreamReader(
                dosya.OpenReadStream(), Encoding.UTF8, true);

            using var csv = new TextFieldParser(okuyucu);

            csv.TextFieldType = FieldType.Delimited;
            csv.SetDelimiters(";");
            csv.HasFieldsEnclosedInQuotes = true;
            csv.TrimWhiteSpace = true;

            var basliklar = csv.ReadFields();

            if (basliklar == null ||
                !basliklar.SequenceEqual(
                    beklenenBasliklar,
                    StringComparer.OrdinalIgnoreCase))
            {
                TempData["Hata"] = "CSV sütun başlıkları hatalı.";
                return RedirectToAction(nameof(Index));
            }

            while (!csv.EndOfData)
            {
                satir++;

                var a = csv.ReadFields();

                if (a == null || a.Length != 12)
                {
                    TempData["Hata"] = $"{satir}. satırda 12 alan olmalı.";
                    return RedirectToAction(nameof(Index));
                }

                if (!int.TryParse(a[9], out int sinif) ||
                    !int.TryParse(a[10], out int stajGunu))
                {
                    TempData["Hata"] = $"{satir}. satırda sayı hatası.";
                    return RedirectToAction(nameof(Index));
                }

                DateTime? dogumTarihi = null;

                if (!string.IsNullOrWhiteSpace(a[7]))
                {
                    if (!DateTime.TryParseExact(
                        a[7], "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime tarih))
                    {
                        TempData["Hata"] =
                            $"{satir}. satırda tarih hatası.";
                        return RedirectToAction(nameof(Index));
                    }

                    dogumTarihi = tarih;
                }

                string yardim = a[11].Trim();

                if (!yardim.Equals("Evet", StringComparison.OrdinalIgnoreCase) &&
                    !yardim.Equals("Hayir", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["Hata"] =
                        $"{satir}. satırda Evet veya Hayir yazılmalı.";
                    return RedirectToAction(nameof(Index));
                }

                var kayit = new OgrenciKaydi
                {
                    OgrenciNo = a[0].Trim(),
                    AdSoyad = a[1].Trim(),
                    Eposta = a[2].Trim(),
                    Telefon = a[3].Trim(),
                    Bolum = a[4].Trim(),
                    KimlikNo = a[5].Trim(),
                    Uyruk = a[6].Trim(),
                    DogumTarihi = dogumTarihi,
                    Adres = a[8].Trim(),
                    Sinif = sinif,
                    OncekiStajGunu = stajGunu,
                    SaglikYardimiAliyor =
                        yardim.Equals("Evet", StringComparison.OrdinalIgnoreCase)
                };

                var hatalar = new List<ValidationResult>();

                if (!Validator.TryValidateObject(
                    kayit, new ValidationContext(kayit), hatalar, true))
                {
                    TempData["Hata"] =
                        $"{satir}. satır geçersiz: {hatalar[0].ErrorMessage}";
                    return RedirectToAction(nameof(Index));
                }

                if (!numaralar.Add(kayit.OgrenciNo))
                {
                    atlanan++;
                    continue;
                }

                yeniKayitlar.Add(kayit);
            }

            await _db.OgrenciKayitlari.AddRangeAsync(yeniKayitlar);
            await _db.SaveChangesAsync();

            TempData["Basari"] =
                $"{yeniKayitlar.Count} öğrenci eklendi. " +
                $"{atlanan} tekrar eden kayıt atlandı.";
        }
        catch (MalformedLineException)
        {
            TempData["Hata"] = "CSV dosyasının biçimi bozuk.";
        }
        catch (DbUpdateException)
        {
            TempData["Hata"] = "Veri tabanına kayıt sırasında hata oluştu.";
        }

        return RedirectToAction(nameof(Index));
    }
}
