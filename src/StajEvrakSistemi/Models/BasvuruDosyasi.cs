namespace StajEvrakSistemi.Models;

public class BasvuruDosyasi
{
    public int Id { get; set; }

    public int BasvuruId { get; set; }
    public Basvuru Basvuru { get; set; } = null!;

    public string OrijinalDosyaAdi { get; set; } = string.Empty;
    public string DepolamaAnahtari { get; set; } = string.Empty;
    public long BoyutBayt { get; set; }
    public DateTime YuklenmeUtc { get; set; } = DateTime.UtcNow;
    public DateTime? KontrolEdilmeUtc { get; set; }

    public List<DogrulamaSonucu> DogrulamaSonuclari { get; set; } = new();
}