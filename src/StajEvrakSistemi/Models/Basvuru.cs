namespace StajEvrakSistemi.Models;

public class Basvuru
{
    public int Id { get; set; }

    public int OgrenciKaydiId { get; set; }
    public OgrenciKaydi OgrenciKaydi { get; set; } = null!;

    public BasvuruDurumu Durum { get; private set; } = BasvuruDurumu.Yuklendi;
    public DateTime OlusturulmaUtc { get; set; } = DateTime.UtcNow;

    public List<BasvuruDosyasi> Dosyalar { get; set; } = new();

    public bool DurumDegistir(BasvuruDurumu yeniDurum)
    {
        bool izinli = (Durum, yeniDurum) switch
        {
            (BasvuruDurumu.Yuklendi, BasvuruDurumu.Inceleniyor) => true,
            (BasvuruDurumu.Inceleniyor, BasvuruDurumu.HataliEksik) => true,
            (BasvuruDurumu.Inceleniyor, BasvuruDurumu.Onaylandi) => true,
            (BasvuruDurumu.HataliEksik, BasvuruDurumu.Yuklendi) => true,
            (BasvuruDurumu.Onaylandi, BasvuruDurumu.Tamamlandi) => true,
            _ => false
        };

        if (!izinli)
        {
            return false;
        }

        Durum = yeniDurum;
        return true;
    }
}