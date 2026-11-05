using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajEvrakSistemi.Controllers;

public class AuthTestController : Controller
{
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return View("Index", "Admin girişi başarılı!");
    }

    [Authorize(Roles = "Ogrenci")]
    public IActionResult Ogrenci()
    {
        return View("Index", "Öğrenci girişi başarılı!");
    }
}