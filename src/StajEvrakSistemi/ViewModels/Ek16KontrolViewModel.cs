
using StajEvrakSistemi.Models;
using StajEvrakSistemi.Services;

namespace StajEvrakSistemi.ViewModels;

public class Ek16KontrolViewModel
{
    public Ek16BasvuruBilgileri Bilgiler { get; set; }
        = new Ek16BasvuruBilgileri();

    public decimal? HesaplananStajGunu { get; set; }

    public List<DogrulamaMesaji> Sonuclar { get; set; }
        = new List<DogrulamaMesaji>();

    public bool KontrolEdildi { get; set; } = false;
}
