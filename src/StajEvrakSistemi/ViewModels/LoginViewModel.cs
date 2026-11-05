
using System.ComponentModel.DataAnnotations;

namespace StajEvrakSistemi.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Eposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    public string Sifre { get; set; } = string.Empty;
}
