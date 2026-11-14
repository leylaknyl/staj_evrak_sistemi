using System.ComponentModel.DataAnnotations;

namespace StajEvrakSistemi.ViewModels;

public class KayitViewModel : LoginViewModel
{
    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [Compare(nameof(Sifre), ErrorMessage = "Şifreler aynı olmalıdır.")]
    [DataType(DataType.Password)]
    public string SifreTekrar { get; set; } = string.Empty;
}