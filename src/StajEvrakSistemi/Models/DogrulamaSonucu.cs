namespace StajEvrakSistemi.Models;

public class DogrulamaSonucu
{
    public int Id { get; set; }

    public int BasvuruDosyasiId { get; set; }
    public BasvuruDosyasi BasvuruDosyasi { get; set; } = null!;

    public string Alan { get; set; } = string.Empty;
    public string Mesaj { get; set; } = string.Empty;
    public bool UyariMi { get; set; }
}