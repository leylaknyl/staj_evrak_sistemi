
using System.Text.RegularExpressions;
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Services;

public class OgretimYiliKurali : DogrulamaKurali
{
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        string yil = bilgi.OgretimYili?.Trim() ?? "";

        // 1. Alan bos mu?
        if (string.IsNullOrWhiteSpace(yil))
        {
            yield return new(
                "OgretimYili",
                "Öğretim yılı boş bırakılamaz.");

            yield break;
        }

        // 2. Yazım biçimi doğru mu?
        var eslesme = Regex.Match(
            yil,
            @"^([0-9]{4})-([0-9]{4}) (Güz|Bahar)$");

        if (!eslesme.Success)
        {
            yield return new(
                "OgretimYili",
                "Öğretim yılı 2026-2027 Bahar biçiminde olmalı.");

            yield break;
        }

        // 3. Yıllar ardışık mı?
        int ilkYil = int.Parse(eslesme.Groups[1].Value);
        int sonYil = int.Parse(eslesme.Groups[2].Value);

        if (sonYil != ilkYil + 1)
        {
            yield return new(
                "OgretimYili",
                "Öğretim yılları ardışık olmalıdır.");
        }
    }
}
