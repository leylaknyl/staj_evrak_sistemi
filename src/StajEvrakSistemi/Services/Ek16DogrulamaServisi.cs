

using System.Text.RegularExpressions;
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Services;

// Hata veya uyarı mesajı
public record DogrulamaMesaji(
    string Alan,
    string Mesaj,
    bool UyariMi = false
);

// SOYUTLAMA
public abstract class DogrulamaKurali
{
    public abstract IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi);
}

// 1. TELEFON VE E-POSTA
public class IletisimKurali : DogrulamaKurali
{
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        if (!string.IsNullOrWhiteSpace(bilgi.Telefon) &&
    !Regex.IsMatch(
        bilgi.Telefon,
        @"^\+90 [0-9]{3} [0-9]{3} [0-9]{2} [0-9]{2}$"))
        {
            yield return new(
                "Telefon",
                "Telefon +90 555 123 45 67 biçiminde olmalı.");
        }

        if (!string.IsNullOrWhiteSpace(bilgi.Eposta) &&
        !Regex.IsMatch(
        bilgi.Eposta,
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            yield return new(
                "Eposta",
                "Geçerli bir e-posta adresi girilmeli.");
        }
    }
}

// 2. KİMLİK NUMARASI
public class KimlikKurali : DogrulamaKurali
{
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        if (string.IsNullOrWhiteSpace(bilgi.KimlikNo) ||
    string.IsNullOrWhiteSpace(bilgi.Uyruk))
{
    yield break;
}
        if (!Regex.IsMatch(bilgi.KimlikNo, @"^[0-9]{11}$"))
        {
            yield return new(
                "KimlikNo",
                "Kimlik numarası 11 rakam olmalı.");

            yield break;
        }

        if (bilgi.Uyruk == "TC")
        {
            if (bilgi.KimlikNo.StartsWith("0"))
                yield return new(
                    "KimlikNo",
                    "TC kimlik numarası 0 ile başlayamaz.");
        }
        else if (bilgi.Uyruk == "Yabanci")
        {
            if (!bilgi.KimlikNo.StartsWith("99"))
                yield return new(
                    "KimlikNo",
                    "Yabancı kimlik numarası 99 ile başlamalı.");
        }
        else
        {
            yield return new(
                "Uyruk",
                "Uyruk TC veya Yabanci olmalı.");
        }
    }
}

// 3. TESLİM TARİHİ
public class TeslimTarihiKurali : DogrulamaKurali
{
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        if (bilgi.TeslimTarihi == null ||
            bilgi.StajBaslangicTarihi == null)
        {
            yield return new(
                "Tarih",
                "Teslim ve staj başlangıç tarihleri zorunlu.");

            yield break;
        }

        DateTime sonTeslimTarihi =
            bilgi.StajBaslangicTarihi.Value.Date.AddDays(-30);

        if (bilgi.TeslimTarihi.Value.Date > sonTeslimTarihi)
        {
            yield return new(
                "TeslimTarihi",
                $"Belge en geç {sonTeslimTarihi:dd.MM.yyyy} " +
                "tarihinde teslim edilmeli.");
        }
    }
}

// 4. MÜHENDİS SAYISI
public class MuhendisKurali : DogrulamaKurali
{
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        if (bilgi.MuhendisSayisi < 1)
        {
            yield return new(
                "MuhendisSayisi",
                "İş yerinde en az 1 mühendis olmalı.");
        }
        else if (bilgi.MuhendisSayisi < 5 &&
                 string.IsNullOrWhiteSpace(
                     bilgi.MuhendisMezunBolumu))
        {
            yield return new(
                "MuhendisMezunBolumu",
                "Mühendisin mezun olduğu bölüm sorulmalı.",
                UyariMi: true);
        }
    }
}

// BÜTÜN KURALLARI ÇALIŞTIRAN SERVİS
public class Ek16DogrulamaServisi
{
    
private readonly List<DogrulamaKurali> _kurallar =
    new List<DogrulamaKurali>
    {
        new ZorunluAlanKurali(),
        new IletisimKurali(),
        new KimlikKurali(),
        new TeslimTarihiKurali(),
        new MuhendisKurali()
    };


    public List<DogrulamaMesaji> Dogrula(
        Ek16BasvuruBilgileri bilgi)
    {
        var mesajlar = new List<DogrulamaMesaji>();

        foreach (var kural in _kurallar)
        {
            mesajlar.AddRange(kural.KontrolEt(bilgi));
        }

        return mesajlar;
    }
}
