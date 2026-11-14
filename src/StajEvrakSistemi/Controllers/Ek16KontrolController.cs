
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using StajEvrakSistemi.Models;
using StajEvrakSistemi.Data;
using StajEvrakSistemi.Services;
using StajEvrakSistemi.ViewModels;

namespace StajEvrakSistemi.Controllers;

[Authorize(Roles = "Ogrenci")]
public class Ek16KontrolController : Controller
{
    private readonly Ek16DogrulamaServisi _servis;
    private readonly StajDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public Ek16KontrolController(
     Ek16DogrulamaServisi servis,
     StajDbContext db,
     UserManager<IdentityUser> userManager)
    {
        _servis = servis;
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
public async Task<IActionResult> Index(int? basvuruId)
{
    if (basvuruId is null)
    {
        return View(new Ek16KontrolViewModel());
    }

    var kullanici = await _userManager.GetUserAsync(User);
    if (kullanici is null)
    {
        return Challenge();
    }

    string eposta = kullanici.Email?.Trim().ToLowerInvariant() ?? "";
    var dosya = await SonBasvuruDosyasi(basvuruId.Value, eposta);

    if (dosya is null)
    {
        return NotFound();
    }

    return View(new Ek16KontrolViewModel
    {
        BasvuruId = basvuruId,
        BasvuruDosyasiId = dosya.Id
    });
}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
    Ek16KontrolViewModel model,
    [FromServices] IWebHostEnvironment ortam)
    {

        var kullanici = await _userManager.GetUserAsync(User);
        if (kullanici is null)
        {
         return Challenge();
        }
        model.Sonuclar = _servis.Dogrula(model.Bilgiler);
        model.HesaplananStajGunu =
            StajGunuKurali.Hesapla(model.Bilgiler);

           string eposta = kullanici.Email?.Trim().ToLowerInvariant() ?? "";

var kayitlar = await _db.OgrenciKayitlari
    .AsNoTracking()
    .Where(o => eposta != "" && o.Eposta.ToLower() == eposta)
    .Take(2)
    .ToListAsync();

if (kayitlar.Count == 1)
{
    var ogrenci = kayitlar[0];
    var bilgi = model.Bilgiler;

    Karsilastir("Ad Soyad", bilgi.AdSoyad, ogrenci.AdSoyad);
    Karsilastir("E-posta", bilgi.Eposta, ogrenci.Eposta);
    Karsilastir("Telefon", YalnizRakamlar(bilgi.Telefon), YalnizRakamlar(ogrenci.Telefon));
    Karsilastir("Uyruk", bilgi.Uyruk, ogrenci.Uyruk);
    Karsilastir("Kimlik Numarası", bilgi.KimlikNo, ogrenci.KimlikNo);
    Karsilastir("Adres", bilgi.Adres, ogrenci.Adres);
}
else
{
    model.Sonuclar.Add(new DogrulamaMesaji(
        "Öğrenci Kaydı",
        "Hesabınıza ait tek bir öğrenci kaydı bulunamadığı için karşılaştırma yapılamadı.",
        UyariMi: true));
}

void Karsilastir(string alan, string? formDegeri, string? kayitDegeri)
{
    if (string.IsNullOrWhiteSpace(kayitDegeri))
    {
        return;
    }

    if (!string.Equals(formDegeri?.Trim(), kayitDegeri.Trim(),
        StringComparison.OrdinalIgnoreCase))
    {
        model.Sonuclar.Add(new DogrulamaMesaji(
            alan, "Ek-16 bilgisi öğrenci kaydıyla uyuşmuyor."));
    }
}
            if (model.BasvuruId.HasValue || model.BasvuruDosyasiId.HasValue)
{
    if (model.BasvuruId is not int basvuruId ||
        model.BasvuruDosyasiId is not int dosyaId)
    {
        return BadRequest();
    }

    if (!ModelState.IsValid)
    {
        ModelState.AddModelError("", "Formdaki eksik veya geçersiz alanları kontrol edin.");
        return View(model);
    }

    await using var islem = await _db.Database.BeginTransactionAsync();

    var dosya = await SonBasvuruDosyasi(basvuruId, eposta);
    if (dosya is null || dosya.Id != dosyaId)
    {
        return Conflict("Başvuru dosyası değişmiş veya artık düzenlenemiyor. Öğrenci panelinden formu yeniden açın.");
    }

    string yol = Path.Combine(
    ortam.ContentRootPath, "App_Data", "Evraklar", dosya.DepolamaAnahtari);

var belge = new Dictionary<string, string>();

try
{
    belge = Ek16BelgeOkuyucu.Oku(yol);
}
catch (Exception hata) when (
    hata is IOException or UnauthorizedAccessException
    or UglyToad.PdfPig.Core.PdfDocumentFormatException
    or System.Xml.XmlException)
{
    model.Sonuclar.Add(new DogrulamaMesaji(
        "Ek-16 belgesi", "Dosya okunamadı. Belgeyi kontrol edin."));
}

if (belge.Count == 0)
{
    model.Sonuclar.Add(new DogrulamaMesaji(
        "Ek-16 alanları", "Belgeden karşılaştırılabilir alan okunamadı."));
}
else
{
    var bilgi = model.Bilgiler;

    BelgeAlaniniKontrolEt("Ad Soyad", bilgi.AdSoyad);
    BelgeAlaniniKontrolEt("E-posta", bilgi.Eposta);
    BelgeAlaniniKontrolEt("Telefon", bilgi.Telefon, YalnizRakamlar);
    BelgeAlaniniKontrolEt("Kimlik Numarası", bilgi.KimlikNo, YalnizRakamlar);
    BelgeAlaniniKontrolEt("Staj Başlangıç Tarihi", bilgi.StajBaslangicTarihi?.ToString("dd.MM.yyyy"));
    BelgeAlaniniKontrolEt("Staj Bitiş Tarihi", bilgi.StajBitisTarihi?.ToString("dd.MM.yyyy"));
    BelgeAlaniniKontrolEt("Evrak Teslim Tarihi", bilgi.TeslimTarihi?.ToString("dd.MM.yyyy"));
}

void BelgeAlaniniKontrolEt(
    string alan, string? formDegeri, Func<string?, string>? normalestir = null)
{
    if (!belge.TryGetValue(alan, out var belgeDegeri))
    {
        model.Sonuclar.Add(new DogrulamaMesaji(alan, "Belgede bu alan okunamadı."));
        return;
    }

    string sol = normalestir?.Invoke(belgeDegeri) ?? belgeDegeri.Trim();
    string sag = normalestir?.Invoke(formDegeri) ?? formDegeri?.Trim() ?? "";

    if (sol.Length == 0 || !string.Equals(sol, sag, StringComparison.OrdinalIgnoreCase))
    {
        model.Sonuclar.Add(new DogrulamaMesaji(
            alan, "Belgedeki değer formdaki değerle uyuşmuyor."));
    }
}
    var oncekiSonuclar = await _db.DogrulamaSonuclari
        .Where(s => s.BasvuruDosyasiId == dosyaId)
        .ToListAsync();

    _db.DogrulamaSonuclari.RemoveRange(oncekiSonuclar);

    _db.DogrulamaSonuclari.AddRange(model.Sonuclar.Select(sonuc => new DogrulamaSonucu
    {
        BasvuruDosyasiId = dosyaId,
        Alan = sonuc.Alan,
        Mesaj = sonuc.Mesaj,
        UyariMi = sonuc.UyariMi
    }));
      dosya.FormBilgileriJson = JsonSerializer.Serialize(model.Bilgiler);
    dosya.KontrolEdilmeUtc = DateTime.UtcNow;
    await _db.SaveChangesAsync();
    await islem.CommitAsync();
}
        model.KontrolEdildi = true;

        return View(model);
    }


     [HttpGet]
public async Task<IActionResult> Sonuclar(int basvuruId)
{
    var kullanici = await _userManager.GetUserAsync(User);
    if (kullanici is null)
    {
        return Challenge();
    }

    string eposta = kullanici.Email?.Trim().ToLowerInvariant() ?? "";
    if (eposta.Length == 0)
    {
        return NotFound();
    }

    var ogrenciIdleri = await _db.OgrenciKayitlari
        .AsNoTracking()
        .Where(o => o.Eposta.ToLower() == eposta)
        .Select(o => o.Id)
        .Take(2)
        .ToListAsync();

    if (ogrenciIdleri.Count != 1)
    {
        return NotFound();
    }

    int ogrenciId = ogrenciIdleri[0];

    var dosya = await _db.BasvuruDosyalari
        .AsNoTracking()
        .Include(d => d.DogrulamaSonuclari)
        .Where(d => d.BasvuruId == basvuruId &&
            d.Basvuru.OgrenciKaydiId == ogrenciId)
        .OrderByDescending(d => d.Id)
        .FirstOrDefaultAsync();

    if (dosya is null)
    {
        return NotFound();
    }

    return View(dosya);
}
    private Task<BasvuruDosyasi?> SonBasvuruDosyasi(int basvuruId, string eposta)
{
    return _db.BasvuruDosyalari
        .Where(d => d.BasvuruId == basvuruId
            && eposta != ""
            && d.Basvuru.OgrenciKaydi.Eposta.ToLower() == eposta
            && d.Basvuru.Durum != BasvuruDurumu.Tamamlandi)
        .OrderByDescending(d => d.Id)
        .FirstOrDefaultAsync();
}
    private static string YalnizRakamlar(string? deger)
{
    return new string((deger ?? "").Where(char.IsDigit).ToArray());
}
}
