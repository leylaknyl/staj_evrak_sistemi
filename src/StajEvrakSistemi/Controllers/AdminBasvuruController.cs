using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using StajEvrakSistemi.Services;
using StajEvrakSistemi.Data;
using StajEvrakSistemi.Models;
using StajEvrakSistemi.ViewModels;

namespace StajEvrakSistemi.Controllers;

[Authorize(Roles = "Admin")]
public class AdminBasvuruController : Controller
{
    private readonly StajDbContext _db;

    public AdminBasvuruController(StajDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var basvurular = await _db.Basvurular
            .AsNoTracking()
            .Include(b => b.OgrenciKaydi)
            .OrderByDescending(b => b.OlusturulmaUtc)
            .ToListAsync();

        return View(basvurular);
    }
     [HttpGet]
public async Task<IActionResult> Belge(
    int id, [FromServices] IWebHostEnvironment ortam)
{
    var dosya = await _db.BasvuruDosyalari
        .AsNoTracking()
        .Include(d => d.Basvuru)
        .ThenInclude(b => b.OgrenciKaydi)
        .Where(d => d.BasvuruId == id)
        .OrderByDescending(d => d.Id)
        .FirstOrDefaultAsync();

    if (dosya is null)
    {
        TempData["Hata"] = "Bu başvuruya ait belge bulunamadı.";
        return RedirectToAction(nameof(Index));
    }

    ViewData["BasvuruId"] = id;
    ViewData["DosyaAdi"] = dosya.OrijinalDosyaAdi;

    var belge = new Dictionary<string, string>();
    try
    {
        string yol = Path.Combine(
            ortam.ContentRootPath, "App_Data", "Evraklar", dosya.DepolamaAnahtari);
        belge = Ek16BelgeOkuyucu.Oku(yol);
    }
    catch (Exception hata) when (
        hata is IOException or UnauthorizedAccessException
        or InvalidDataException or System.Xml.XmlException
        or UglyToad.PdfPig.Core.PdfDocumentFormatException)
    {
        ViewData["OkumaMesaji"] = "Belge okunamadı.";
    }

    if (belge.Count == 0)
    {
        ViewData["OkumaMesaji"] ??= "Belgeden karşılaştırılabilir alan okunamadı.";
    }

    var form = dosya.FormBilgileriJson is string json
        ? JsonSerializer.Deserialize<Ek16BasvuruBilgileri>(json)
        : null;
    var ogrenci = dosya.Basvuru.OgrenciKaydi;

    return View(new[]
    {
        Satir("Ad Soyad", form?.AdSoyad, ogrenci.AdSoyad),
        Satir("E-posta", form?.Eposta, ogrenci.Eposta),
        Satir("Telefon", form?.Telefon, ogrenci.Telefon),
        Satir("Öğrenci No", null, ogrenci.OgrenciNo),
        Satir("Kimlik Numarası", form?.KimlikNo, ogrenci.KimlikNo)
    });

      AdminBelgeSatiri Satir(string alan, string? formDegeri, string? csv)
    {
        string? okunan = belge.GetValueOrDefault(alan);
        string sol = okunan?.Trim() ?? "";
        string sag = csv?.Trim() ?? "";

        if (alan == "Telefon")
        {
            sol = new string(sol.Where(char.IsDigit).ToArray());
            sag = new string(sag.Where(char.IsDigit).ToArray());
        }

        string sonuc = sol.Length == 0 || sag.Length == 0
            ? "Veri eksik"
            : string.Equals(sol, sag, StringComparison.OrdinalIgnoreCase)
                ? "Uyumlu"
                : "Uyuşmuyor";

        return new AdminBelgeSatiri(alan, okunan, formDegeri, csv, sonuc);
    }
}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> IncelemeyeAl(int id)
        => DurumuGuncelle(id, BasvuruDurumu.Inceleniyor);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Onayla(int id)
        => DurumuGuncelle(id, BasvuruDurumu.Onaylandi);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> HataliEksikIsaretle(int id)
        => DurumuGuncelle(id, BasvuruDurumu.HataliEksik);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Tamamla(int id)
        => DurumuGuncelle(id, BasvuruDurumu.Tamamlandi);    

    private async Task<IActionResult> DurumuGuncelle(int id, BasvuruDurumu hedefDurum)
    {
        var basvuru = await _db.Basvurular
            .AsNoTracking()
            .SingleOrDefaultAsync(b => b.Id == id);

        if (basvuru is null)
        {
            return NotFound();
        }

        var oncekiDurum = basvuru.Durum;

        if (!basvuru.DurumDegistir(hedefDurum))
        {
            TempData["Hata"] = "Başvurunun mevcut durumu bu işleme izin vermiyor.";
            return RedirectToAction(nameof(Index));
        }

        var guncellenenKayit = await _db.Basvurular
            .Where(b => b.Id == id && b.Durum == oncekiDurum)
            .ExecuteUpdateAsync(guncelle => guncelle.SetProperty(b => b.Durum, hedefDurum));

        if (guncellenenKayit == 0)
        {
            TempData["Hata"] = "Başvuru durumu değişmiş. Listeyi kontrol edin.";
        }

        return RedirectToAction(nameof(Index));
    }
}