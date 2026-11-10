
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StajEvrakSistemi.Services;
using StajEvrakSistemi.ViewModels;

namespace StajEvrakSistemi.Controllers;

[Authorize(Roles = "Ogrenci")]
public class Ek16KontrolController : Controller
{
    private readonly Ek16DogrulamaServisi _servis;

    public Ek16KontrolController(
        Ek16DogrulamaServisi servis)
    {
        _servis = servis;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new Ek16KontrolViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(Ek16KontrolViewModel model)
    {
        model.Sonuclar = _servis.Dogrula(model.Bilgiler);

        model.KontrolEdildi = true;

        return View(model);
    }
}
