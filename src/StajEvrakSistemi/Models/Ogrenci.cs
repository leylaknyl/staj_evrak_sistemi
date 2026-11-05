
namespace StajEvrakSistemi.Models;

public class Ogrenci : Kullanici
{
    public string OgrenciNo { get; set; } = string.Empty;

    public string Bolum { get; set; } = string.Empty;

    public Ogrenci(
        string adSoyad,
        string eposta,
        string ogrenciNo,
        string bolum)
    {
        this.AdSoyad = adSoyad;
        this.Eposta = eposta;
        this.OgrenciNo = ogrenciNo;
        this.Bolum = bolum;
    }
}

