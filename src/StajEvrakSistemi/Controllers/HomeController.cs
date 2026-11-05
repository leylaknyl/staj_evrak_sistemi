using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult OgrenciOrnegi()
    {
        Ogrenci ogrenci1 = new Ogrenci(
            "Ayşe Yılmaz",
            "ayse@example.com",
            "1001",
            "Yazılım Mühendisliği"
        );

        return Content(
            $"Öğrenci: {ogrenci1.AdSoyad} | " +
            $"Numara: {ogrenci1.OgrenciNo} | " +
            $"Bölüm: {ogrenci1.Bolum}"
        );
    }



    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
