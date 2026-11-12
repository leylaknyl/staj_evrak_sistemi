
using System.Text.RegularExpressions;
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Services;

public class AdresKurali : DogrulamaKurali
{
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        string adres = bilgi.Adres?.Trim() ?? "";

        // Bos adresi zaten ZorunluAlanKurali kontrol ediyor.
        if (string.IsNullOrWhiteSpace(adres))
            yield break;

        // Adresi virgüllerden ayır.
        string[] parcalar = adres.Split(
            ',',
            StringSplitOptions.TrimEntries
        );

        // Beklenen sıra:
        // Mahalle, Cadde/Sokak, Bina No, Daire No, İlçe/İl
        if (parcalar.Length != 5)
        {
            yield return new(
                "Adres",
                "Adres sırası: Mahalle, Cadde/Sokak, " +
                "Bina No, Daire No, İlçe/İl olmalıdır.");

            yield break;
        }

        if (!Regex.IsMatch(
            parcalar[0],
            @"Mahalle|Mah\.",
            RegexOptions.IgnoreCase))
        {
            yield return new(
                "Adres",
                "Adresin ilk bölümünde mahalle bulunmalıdır.");
        }

        if (!Regex.IsMatch(
            parcalar[1],
            @"Cadde|Caddesi|Sokak|Sokağı|Cd\.|Sk\.",
            RegexOptions.IgnoreCase))
        {
            yield return new(
                "Adres",
                "Adresin ikinci bölümünde cadde veya sokak bulunmalıdır.");
        }

        if (!Regex.IsMatch(
            parcalar[2],
            @"^Bina\s+No\s*:?\s*\S+",
            RegexOptions.IgnoreCase))
        {
            yield return new(
                "Adres",
                "Bina numarası 'Bina No: 12' biçiminde yazılmalıdır.");
        }

        if (!Regex.IsMatch(
            parcalar[3],
            @"^Daire\s+No\s*:?\s*\S+",
            RegexOptions.IgnoreCase))
        {
            yield return new(
                "Adres",
                "Daire numarası 'Daire No: 3' biçiminde yazılmalıdır.");
        }

        string[] konum = parcalar[4].Split('/');

        if (konum.Length != 2 ||
            string.IsNullOrWhiteSpace(konum[0]) ||
            string.IsNullOrWhiteSpace(konum[1]))
        {
            yield return new(
                "Adres",
                "İlçe ve il 'İzmit/Kocaeli' biçiminde yazılmalıdır.");
        }
    }
}
