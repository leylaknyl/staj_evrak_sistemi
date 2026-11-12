
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Services;

public class ZorunluAlanKurali : DogrulamaKurali
{
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        if (string.IsNullOrWhiteSpace(bilgi.AdSoyad))
        {
            yield return new(
                "AdSoyad",
                "Ad Soyad alanı boş bırakılamaz.");
        }

        if (string.IsNullOrWhiteSpace(bilgi.Eposta))
        {
            yield return new(
                "Eposta",
                "E-posta alanı zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(bilgi.Telefon))
        {
            yield return new(
                "Telefon",
                "Telefon numarası zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(bilgi.Uyruk))
        {
            yield return new(
                "Uyruk",
                "Uyruk seçilmelidir.");
        }

        if (string.IsNullOrWhiteSpace(bilgi.KimlikNo))
        {
            yield return new(
                "KimlikNo",
                "Kimlik numarası zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(bilgi.Adres))
        {
            yield return new(
                "Adres",
                "Adres alanı boş bırakılamaz.");
        }
    }
}
