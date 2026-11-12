
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

        string eposta = kullanici.Email?.Trim().ToLower() ?? "";

        var ogrenciKaydi = await _db.OgrenciKayitlari
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Eposta.ToLower() == eposta);

        var model = new OgrenciPanelViewModel
        {
            Eposta = kullanici.Email ?? "",
            Ogrenci = ogrenciKaydi
        };

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
