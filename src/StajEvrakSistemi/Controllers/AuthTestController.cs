
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajEvrakSistemi.Controllers;

public class AuthTestController : Controller
{
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return RedirectToAction("Admin", "Paneller");
    }

    [Authorize(Roles = "Ogrenci")]
    public IActionResult Ogrenci()
    {
        return RedirectToAction("Ogrenci", "Paneller");
    }
}
