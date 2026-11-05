
using Microsoft.AspNetCore.Identity;

namespace StajEvrakSistemi.Data;

public static class AuthSeed
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<UserManager<IdentityUser>>();

        string[] roller = { "Admin", "Ogrenci" };

        foreach (var rol in roller)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                var sonuc = await roleManager.CreateAsync(
                    new IdentityRole(rol));

                if (!sonuc.Succeeded)
                    throw new Exception("Rol oluşturulamadı.");
            }
        }

        string sifre = configuration["DemoAccounts:Password"]
            ?? throw new Exception(
                "Demo şifresi tanımlanmamış.");

        await KullaniciEkle(
            userManager,
            "admin@example.com",
            sifre,
            "Admin");

        await KullaniciEkle(
            userManager,
            "ogrenci@example.com",
            sifre,
            "Ogrenci");
    }

    private static async Task KullaniciEkle(
        UserManager<IdentityUser> userManager,
        string eposta,
        string sifre,
        string rol)
    {
        var kullanici = await userManager.FindByEmailAsync(eposta);

        if (kullanici == null)
        {
            kullanici = new IdentityUser
            {
                UserName = eposta,
                Email = eposta,
                EmailConfirmed = true
            };

            var sonuc = await userManager.CreateAsync(
                kullanici, sifre);

            if (!sonuc.Succeeded)
            {
                string hatalar = string.Join(
                    ", ",
                    sonuc.Errors.Select(h => h.Description));

                throw new Exception(hatalar);
            }
        }

        if (!await userManager.IsInRoleAsync(kullanici, rol))
        {
            var sonuc = await userManager.AddToRoleAsync(
                kullanici, rol);

            if (!sonuc.Succeeded)
                throw new Exception("Rol atanamadı.");
        }
    }
}
