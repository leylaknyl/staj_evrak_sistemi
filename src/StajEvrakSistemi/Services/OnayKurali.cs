
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Services;

public class OnayKurali : DogrulamaKurali
{
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        // Öğrenci onay kısmı boş kalmalı.
        if (bilgi.OgrenciOnayiVar)
        {
            yield return new(
                "OgrenciOnayi",
                "Öğrenci onay alanı boş bırakılmalıdır.");
        }

        // Staj danışmanı onay kısmı boş kalmalı.
        if (bilgi.DanismanOnayiVar)
        {
            yield return new(
                "DanismanOnayi",
                "Staj danışmanı onay alanı boş bırakılmalıdır.");
        }

        // İşveren kısmı tamamen dolu olmalı.
        if (!bilgi.IsverenBilgileriDolu)
        {
            yield return new(
                "IsverenBilgileri",
                "İşveren bölümündeki bilgiler eksiksiz doldurulmalıdır.");
        }

        // İşveren imzası zorunlu.
        if (!bilgi.IsverenImzasiVar)
        {
            yield return new(
                "IsverenImzasi",
                "İşveren imzası eksiktir.");
        }

        // İşveren kaşesi zorunlu.
        if (!bilgi.IsverenKasesiVar)
        {
            yield return new(
                "IsverenKasesi",
                "İşveren kaşesi eksiktir.");
        }
    }
}
