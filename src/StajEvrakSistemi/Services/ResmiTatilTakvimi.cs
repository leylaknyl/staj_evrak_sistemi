
namespace StajEvrakSistemi.Services;

public static class ResmiTatilTakvimi
{
    // Şimdilik 2026 ve 2027 takvimleri tanımlı.
    public static bool Destekleniyor(int yil)
    {
        return yil == 2026 || yil == 2027;
    }

    // 1 = tam çalışma günü
    // 0.5 = yarım çalışma günü
    // 0 = resmî tatil
    public static decimal GunKatsayisi(DateTime tarih)
    {
        tarih = tarih.Date;

        if (!Destekleniyor(tarih.Year))
            throw new ArgumentOutOfRangeException(
                nameof(tarih),
                "Bu yılın resmî tatil takvimi tanımlanmamış.");

        // Sabit resmî tatiller
        bool sabitTatil =
            (tarih.Month == 1 && tarih.Day == 1) ||
            (tarih.Month == 4 && tarih.Day == 23) ||
            (tarih.Month == 5 && tarih.Day == 1) ||
            (tarih.Month == 5 && tarih.Day == 19) ||
            (tarih.Month == 7 && tarih.Day == 15) ||
            (tarih.Month == 8 && tarih.Day == 30) ||
            (tarih.Month == 10 && tarih.Day == 29);

        if (sabitTatil)
            return 0m;

        DateTime ramazanBaslangic;
        DateTime kurbanBaslangic;

        if (tarih.Year == 2026)
        {
            ramazanBaslangic = new DateTime(2026, 3, 20);
            kurbanBaslangic = new DateTime(2026, 5, 27);
        }
        else
        {
            ramazanBaslangic = new DateTime(2027, 3, 9);
            kurbanBaslangic = new DateTime(2027, 5, 16);
        }

        // Ramazan Bayramı: 3 gün
        if (tarih >= ramazanBaslangic &&
            tarih < ramazanBaslangic.AddDays(3))
        {
            return 0m;
        }

        // Kurban Bayramı: 4 gün
        if (tarih >= kurbanBaslangic &&
            tarih < kurbanBaslangic.AddDays(4))
        {
            return 0m;
        }

        // Arife günleri ve 28 Ekim yarım gündür.
        if (tarih == ramazanBaslangic.AddDays(-1) ||
            tarih == kurbanBaslangic.AddDays(-1) ||
            (tarih.Month == 10 && tarih.Day == 28))
        {
            return 0.5m;
        }

        return 1m;
    }
}
