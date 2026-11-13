using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajEvrakSistemi.Data;
using StajEvrakSistemi.Models;

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