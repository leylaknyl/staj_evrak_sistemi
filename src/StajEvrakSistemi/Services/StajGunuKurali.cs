
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.Services;

public class StajGunuKurali : DogrulamaKurali
{
    private static HashSet<DayOfWeek> SecilenGunler(
        Ek16BasvuruBilgileri bilgi)
    {
        var gunler = new HashSet<DayOfWeek>();

        if (bilgi.Pazartesi)
            gunler.Add(DayOfWeek.Monday);

        if (bilgi.Sali)
            gunler.Add(DayOfWeek.Tuesday);

        if (bilgi.Carsamba)
            gunler.Add(DayOfWeek.Wednesday);

        if (bilgi.Persembe)
            gunler.Add(DayOfWeek.Thursday);

        if (bilgi.Cuma)
            gunler.Add(DayOfWeek.Friday);

        if (bilgi.Cumartesi)
            gunler.Add(DayOfWeek.Saturday);

        if (bilgi.Pazar)
            gunler.Add(DayOfWeek.Sunday);

        return gunler;
    }

    // Staj günlerini hesaplayan metot
    public static decimal? Hesapla(
        Ek16BasvuruBilgileri bilgi)
    {
        if (bilgi.StajBaslangicTarihi == null ||
            bilgi.StajBitisTarihi == null)
            return null;

        DateTime baslangic =
            bilgi.StajBaslangicTarihi.Value.Date;

        DateTime bitis =
            bilgi.StajBitisTarihi.Value.Date;

        if (bitis < baslangic)
            return null;

        // Çok büyük tarih aralıklarını engelle
        if ((bitis - baslangic).TotalDays > 366)
            return null;

        var gunler = SecilenGunler(bilgi);

        if (gunler.Count == 0)
            return null;

        // Tanımsız tatil takvimleriyle hesap yapma.
        for (int yil = baslangic.Year;
             yil <= bitis.Year;
             yil++)
        {
            if (!ResmiTatilTakvimi.Destekleniyor(yil))
                return null;
        }

        decimal toplam = 0m;

        for (DateTime tarih = baslangic;
             tarih <= bitis;
             tarih = tarih.AddDays(1))
        {
            // Sadece öğrencinin seçtiği günleri say.
            if (!gunler.Contains(tarih.DayOfWeek))
                continue;

            // Resmî tatilse 0, yarım günse 0.5 ekle.
            toplam += ResmiTatilTakvimi.GunKatsayisi(tarih);
        }

        return toplam;
    }

    // Doğrulama sonucunu üret
    public override IEnumerable<DogrulamaMesaji>
        KontrolEt(Ek16BasvuruBilgileri bilgi)
    {
        if (bilgi.StajBaslangicTarihi == null ||
            bilgi.StajBitisTarihi == null)
        {
            yield return new(
                "StajTarihleri",
                "Staj başlangıç ve bitiş tarihleri zorunludur.");

            yield break;
        }

        if (bilgi.StajBitisTarihi <
            bilgi.StajBaslangicTarihi)
        {
            yield return new(
                "StajBitisTarihi",
                "Bitiş tarihi başlangıçtan önce olamaz.");

            yield break;
        }

        if ((bilgi.StajBitisTarihi.Value.Date -
             bilgi.StajBaslangicTarihi.Value.Date)
             .TotalDays > 366)
        {
            yield return new(
                "StajTarihleri",
                "Hesaplama aralığı 366 günü aşamaz.");

            yield break;
        }

        if (SecilenGunler(bilgi).Count == 0)
        {
            yield return new(
                "CalismaGunleri",
                "En az bir çalışma günü seçmelisiniz.");

            yield break;
        }

        decimal? hesaplanan = Hesapla(bilgi);

        if (hesaplanan == null)
        {
            yield return new(
                "StajTarihleri",
                "Seçilen yılların resmî tatil takvimi tanımlı değil.");

            yield break;
        }

        if (hesaplanan.Value == 0m)
        {
            yield return new(
                "StajGunSayisi",
                "Seçilen aralıkta geçerli staj günü bulunamadı.");
        }

        if (bilgi.BelgedekiStajGunSayisi == null)
        {
            yield return new(
                "BelgedekiStajGunSayisi",
                "Belgedeki staj gün sayısı girilmelidir.");

            yield break;
        }

        if (bilgi.BelgedekiStajGunSayisi <= 0m)
        {
            yield return new(
                "BelgedekiStajGunSayisi",
                "Belgedeki gün sayısı sıfırdan büyük olmalıdır.");

            yield break;
        }

        if (bilgi.BelgedekiStajGunSayisi != hesaplanan.Value)
        {
            yield return new(
                "StajGunSayisi",
                $"Hesaplanan gün: {hesaplanan.Value:0.##}. " +
                $"Belgede yazan gün: " +
                $"{bilgi.BelgedekiStajGunSayisi.Value:0.##}. " +
                "Staj gün sayıları uyuşmuyor.");
        }
    }
}
