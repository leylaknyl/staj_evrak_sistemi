

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
        string kimlik = bilgi.KimlikNo?.Trim() ?? "";
        string uyruk = bilgi.Uyruk?.Trim() ?? "";

        // Bos alanlar ZorunluAlanKurali tarafindan kontrol edilir.
        if (string.IsNullOrWhiteSpace(kimlik) ||
            string.IsNullOrWhiteSpace(uyruk))
        {
            yield break;
        }

        // Her iki kimlik turu de 11 rakam olmali.
        if (!Regex.IsMatch(kimlik, @"^[0-9]{11}$"))
        {
            yield return new(
                "KimlikNo",
                "Kimlik numarasi 11 rakamdan olusmalidir.");

            yield break;
        }

        if (uyruk == "TC")
        {
            // TC kimlik numarasi sifirla baslayamaz.
            if (kimlik[0] == '0')
            {
                yield return new(
                    "KimlikNo",
                    "TC kimlik numarasi 0 ile baslayamaz.");

                yield break;
            }

            // Karakterleri rakama donustur.
            int[] rakamlar = kimlik
                .Select(c => c - '0')
                .ToArray();

            // 1, 3, 5, 7, 9. basamaklar
            int tekToplam =
                rakamlar[0] +
                rakamlar[2] +
                rakamlar[4] +
                rakamlar[6] +
                rakamlar[8];

            // 2, 4, 6, 8. basamaklar
            int ciftToplam =
                rakamlar[1] +
                rakamlar[3] +
                rakamlar[5] +
                rakamlar[7];

            // 10. kontrol basamagi
            int onuncuBasamak =
                (((tekToplam * 7 - ciftToplam) % 10) + 10) % 10;

            // 11. kontrol basamagi
            int onBirinciBasamak =
                rakamlar.Take(10).Sum() % 10;

            if (rakamlar[9] != onuncuBasamak ||
                rakamlar[10] != onBirinciBasamak)
            {
                yield return new(
                    "KimlikNo",
                    "TC kimlik numarasinin kontrol basamaklari hatali.");
            }
        }
        else if (uyruk == "Yabanci")
        {
            if (!kimlik.StartsWith("99"))
            {
                yield return new(
                    "KimlikNo",
                    "Yabanci kimlik numarasi 99 ile baslamalidir.");
            }
        }
        else
        {
            yield return new(
                "Uyruk",
                "Gecerli bir uyruk secilmelidir.");
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
