using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajEvrakSistemi.Services;
using StajEvrakSistemi.Data;
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Controllers;

[Authorize(Roles = "Ogrenci")]
public class EvrakController(
    StajDbContext db,
    UserManager<IdentityUser> userManager,
    IWebHostEnvironment ortam,
    ILogger<EvrakController> logger) : Controller
{
    private const long EnFazlaDosyaBoyutu = 10 * 1024 * 1024;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Yukle(IFormFile? dosya, int? basvuruId)
    {
    if (!ModelState.IsValid)
{
    return YuklemeHatasi("Yükleme isteği geçersiz.");
}
        var kullanici = await userManager.GetUserAsync(User);
        if (kullanici is null)
        {
            return Challenge();
        }

        string eposta = kullanici.Email?.Trim().ToLowerInvariant() ?? "";
        var ogrenciIdleri = await db.OgrenciKayitlari
            .Where(o => eposta != "" && o.Eposta.ToLower() == eposta)
            .Select(o => o.Id)
            .Take(2)
            .ToListAsync();

        if (ogrenciIdleri.Count != 1)
        {
            return YuklemeHatasi("Hesabınıza ait tek bir öğrenci kaydı bulunamadı.");
        }

        if (dosya is null || dosya.Length == 0 ||
            dosya.Length > EnFazlaDosyaBoyutu)
        {
            return YuklemeHatasi("Boş olmayan, en fazla 10 MB boyutunda bir dosya seçin.");
        }

        string dosyaAdi = Path.GetFileName(dosya.FileName.Replace('\\', '/'));
        string uzanti = Path.GetExtension(dosyaAdi).ToLowerInvariant();

        if (uzanti is not (".pdf" or ".doc" or ".docx" or ".jpg" or ".jpeg" or ".png"))
        {
            return YuklemeHatasi("PDF, Word, JPEG veya PNG dosyası seçin.");
        }
        using (var akim = dosya.OpenReadStream())
{
    if (!Ek16BelgeOkuyucu.FormatGecerli(akim, uzanti))
    {
        return YuklemeHatasi("Dosyanın içeriği seçilen uzantıyla uyuşmuyor.");
    }
}

        string klasor = Path.Combine(ortam.ContentRootPath, "App_Data", "Evraklar");
        string anahtar = $"{Guid.NewGuid():N}{uzanti}";
        string yol = Path.Combine(klasor, anahtar);

        Basvuru basvuru;

if (basvuruId is int id)
{
    var mevcutBasvuru = await db.Basvurular.SingleOrDefaultAsync(
        b => b.Id == id && b.OgrenciKaydiId == ogrenciIdleri[0]);

    if (mevcutBasvuru is null ||
        !mevcutBasvuru.DurumDegistir(BasvuruDurumu.Yuklendi))
    {
        return YuklemeHatasi("Bu başvuruya yeniden evrak yüklenemiyor.");
    }

    basvuru = mevcutBasvuru;
}
else
{
    basvuru = new Basvuru
    {
        OgrenciKaydiId = ogrenciIdleri[0]
    };
}

        basvuru.Dosyalar.Add(new BasvuruDosyasi
        {
            OrijinalDosyaAdi = dosyaAdi,
            DepolamaAnahtari = anahtar,
            BoyutBayt = dosya.Length
        });

        bool dosyaOlusturuldu = false;

        try
        {
            Directory.CreateDirectory(klasor);

            await using (var hedef = new FileStream(yol, FileMode.CreateNew))
            {
                dosyaOlusturuldu = true;
                await dosya.CopyToAsync(hedef);
            }

            if (basvuruId is null)
{
    db.Basvurular.Add(basvuru);
}
            await db.SaveChangesAsync();
        }
        catch (Exception hata) when (
            hata is IOException or UnauthorizedAccessException or DbUpdateException)
        {
            logger.LogError(hata, "Ek-16 yüklemesi kaydedilemedi.");

            if (dosyaOlusturuldu)
            {
                try
                {
                    System.IO.File.Delete(yol);
                }
                catch (Exception silmeHatasi) when (
                    silmeHatasi is IOException or UnauthorizedAccessException)
                {
                    logger.LogWarning(silmeHatasi, "Başarısız yüklemenin dosyası silinemedi.");
                }
            }

            return YuklemeHatasi("Evrak kaydedilemedi. Lütfen tekrar deneyin.");
        }

        TempData["EvrakBasari"] = $"Ek-16 yüklendi. Başvuru numaranız: {basvuru.Id}";
        return RedirectToAction("Ogrenci", "Paneller");
    }

    private IActionResult YuklemeHatasi(string mesaj)
    {
        TempData["EvrakHata"] = mesaj;
        return RedirectToAction("Ogrenci", "Paneller");
    }
}