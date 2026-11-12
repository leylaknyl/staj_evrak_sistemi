
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


    public DateTime? StajBitisTarihi { get; set; }

// Öğrencinin seçtiği haftalık çalışma günleri
    public bool Pazartesi { get; set; }
    public bool Sali { get; set; }
    public bool Carsamba { get; set; }
    public bool Persembe { get; set; }
    public bool Cuma { get; set; }
    public bool Cumartesi { get; set; }
    public bool Pazar { get; set; }

// Ek-16 belgesinde bildirilen staj günü
    public decimal? BelgedekiStajGunSayisi { get; set; }


    public string Faks { get; set; } = string.Empty;

// Ek-16 belgesi üzerindeki onay alanları
    public bool OgrenciOnayiVar { get; set; }

    public bool DanismanOnayiVar { get; set; }

    public bool IsverenBilgileriDolu { get; set; }

    public bool IsverenImzasiVar { get; set; }

    public bool IsverenKasesiVar { get; set; }


}
