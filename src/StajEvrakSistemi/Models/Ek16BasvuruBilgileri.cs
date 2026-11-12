
namespace StajEvrakSistemi.Models;

public class Ek16BasvuruBilgileri
{
    public string AdSoyad { get; set; } = string.Empty;

    public string Eposta { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;

    // "TC" veya "Yabanci"
    public string Uyruk { get; set; } = string.Empty;

    public string KimlikNo { get; set; } = string.Empty;

    public string Adres { get; set; } = string.Empty;

    public DateTime? TeslimTarihi { get; set; }

    public DateTime? StajBaslangicTarihi { get; set; }

    public int MuhendisSayisi { get; set; }

    public string MuhendisMezunBolumu { get; set; }
        = string.Empty;
    public string OgretimYili { get; set; } = string.Empty;
}
