
using System.ComponentModel.DataAnnotations;

namespace StajEvrakSistemi.Models;

public class OgrenciKaydi
{
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string OgrenciNo { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string AdSoyad { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Eposta { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Telefon { get; set; } = string.Empty;

    [Required]
    public string Bolum { get; set; } = string.Empty;

    [MaxLength(20)]
    public string KimlikNo { get; set; } = string.Empty;

    public string Uyruk { get; set; } = string.Empty;

    public DateTime? DogumTarihi { get; set; }

    public string Adres { get; set; } = string.Empty;

    [Range(1, 6)]
    public int Sinif { get; set; }

    [Range(0, 365)]
    public int OncekiStajGunu { get; set; }

    public bool SaglikYardimiAliyor { get; set; }
}
