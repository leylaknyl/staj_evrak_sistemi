
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajEvrakSistemi.Data;
using StajEvrakSistemi.ViewModels;

namespace StajEvrakSistemi.Controllers;

[Authorize]
public class PanellerController : Controller
{
    private readonly StajDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public PanellerController(
        StajDbContext db,
        UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // ÖĞRENCİ PANELİ
    [Authorize(Roles = "Ogrenci")]
    [HttpGet]
    public async Task<IActionResult> Ogrenci()
    {
        var kullanici = await _userManager.GetUserAsync(User);

        if (kullanici == null)
        {
            return Challenge();
        }

        string eposta = kullanici.Email?.Trim().ToLowerInvariant() ?? "";

var ogrenciKayitlari = await _db.OgrenciKayitlari
    .AsNoTracking()
    .Where(o => eposta != "" && o.Eposta.ToLower() == eposta)
    .Take(2)
    .ToListAsync();

var ogrenciKaydi = ogrenciKayitlari.Count == 1 ? ogrenciKayitlari[0] : null;

var model = new OgrenciPanelViewModel
{
    Eposta = kullanici.Email ?? "",
    Ogrenci = ogrenciKaydi
};

if (ogrenciKaydi is not null)
{
    model.Basvurular = await _db.Basvurular
        .AsNoTracking()
        .Where(b => b.OgrenciKaydiId == ogrenciKaydi.Id)
        .OrderByDescending(b => b.Id)
        .Select(b => new OgrenciBasvuruSatiri(
            b.Id,
            b.Dosyalar
                .OrderByDescending(d => d.Id)
                .Select(d => d.OrijinalDosyaAdi)
                .FirstOrDefault(),
            b.Durum))
        .ToListAsync();
}

return View(model);
    }
    // ADMİN PANELİ
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Admin()
    {
        var model = new AdminPanelViewModel
        {
            ToplamOgrenci = await _db.OgrenciKayitlari
                .CountAsync(),

            SonOgrenciler = await _db.OgrenciKayitlari
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }
}
