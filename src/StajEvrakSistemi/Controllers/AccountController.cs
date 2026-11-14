
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StajEvrakSistemi.ViewModels;

namespace StajEvrakSistemi.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(
            model.Eposta.Trim());

        if (user == null)
        {
            ModelState.AddModelError(
                "",
                "E-posta veya şifre hatalı.");

            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Sifre,
            isPersistent: false,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                "",
                "Hesabınız geçici olarak kilitlendi.");

            return View(model);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                "",
                "E-posta veya şifre hatalı.");

            return View(model);
        }

        
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return RedirectToAction("Admin", "AuthTest");
        }

        if (await _userManager.IsInRoleAsync(user, "Ogrenci"))
        {
            return RedirectToAction("Ogrenci", "AuthTest");
        }

        await _signInManager.SignOutAsync();

        ModelState.AddModelError(
            "",
            "Hesabınız için geçerli bir rol tanımlanmamış.");

        return View(model);

    }


      [AllowAnonymous]
[HttpGet]
public IActionResult Kayit()
{
    return View(new KayitViewModel());
}

[AllowAnonymous]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Kayit(KayitViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    string eposta = model.Eposta.Trim();
    var kullanici = new IdentityUser
    {
        UserName = eposta,
        Email = eposta
    };

    var sonuc = await _userManager.CreateAsync(kullanici, model.Sifre);

    if (sonuc.Succeeded)
    {
        sonuc = await _userManager.AddToRoleAsync(kullanici, "Ogrenci");

        if (sonuc.Succeeded)
        {
            return RedirectToAction(nameof(Login));
        }

        var geriAlma = await _userManager.DeleteAsync(kullanici);
        if (!geriAlma.Succeeded)
        {
            ModelState.AddModelError("", "Hesap kurulumu tamamlanamadı. Yöneticiye başvurun.");
            return View(model);
        }
    }

    foreach (var hata in sonuc.Errors)
    {
        ModelState.AddModelError("", hata.Description);
    }

    return View(model);
}
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
