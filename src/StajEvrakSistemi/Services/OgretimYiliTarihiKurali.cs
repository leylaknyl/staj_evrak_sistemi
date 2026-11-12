
using System.Text.RegularExpressions;
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Services;

public class OgretimYiliTarihKurali : DogrulamaKurali
{
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        string yil = bilgi.OgretimYili?.Trim() ?? "";

        // Format kontrolünü OgretimYiliKurali yapıyor.
        var eslesme = Regex.Match(
            yil,
            @"^([0-9]{4})-([0-9]{4}) (Güz|Bahar)$");

        if (!eslesme.Success)
            yield break;

        int ilkYil = int.Parse(eslesme.Groups[1].Value);
        int sonYil = int.Parse(eslesme.Groups[2].Value);

        // Geçersiz yıllar diğer kuralda raporlanır.
        if (ilkYil < 1 || sonYil > 9999 ||
            sonYil != ilkYil + 1)
            yield break;

        if (bilgi.StajBaslangicTarihi == null ||
            bilgi.StajBitisTarihi == null)
            yield break;

        // Akademik yıl için Eylül-Ağustos aralığı.
        DateTime donemBaslangici =
            new DateTime(ilkYil, 9, 1);

        DateTime donemBitisi =
            new DateTime(sonYil, 8, 31);

        DateTime stajBaslangici =
            bilgi.StajBaslangicTarihi.Value.Date;

        DateTime stajBitisi =
            bilgi.StajBitisTarihi.Value.Date;

        if (stajBaslangici < donemBaslangici ||
            stajBitisi > donemBitisi)
        {
            yield return new(
                "OgretimYili",
                $"Staj tarihleri {ilkYil}-{sonYil} " +
                "akademik yılı dışında kalıyor.");
        }
    }
}
