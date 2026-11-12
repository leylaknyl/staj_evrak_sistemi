
using StajEvrakSistemi.Models;

namespace StajEvrakSistemi.ViewModels;

// Öğrenci panelinin kullanacağı veriler
public class OgrenciPanelViewModel
{
    public string Eposta { get; set; } = string.Empty;

    public OgrenciKaydi? Ogrenci { get; set; }
}

// Admin panelinin kullanacağı veriler
public class AdminPanelViewModel
{
    public int ToplamOgrenci { get; set; }

    public List<OgrenciKaydi> SonOgrenciler { get; set; }
        = new List<OgrenciKaydi>();
}
