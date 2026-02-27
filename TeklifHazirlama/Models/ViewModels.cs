using System.ComponentModel.DataAnnotations;

namespace TeklifHazirlama.Models;

public class TesisatGrubuInput
{
    [Required]
    [Display(Name = "Tesisat Grubu")]
    public string Ad { get; set; } = string.Empty;
}

public class IsDetayiInput
{
    public int TesisatGrubuId { get; set; }

    [Required]
    [Display(Name = "İş Detayı")]
    public string Ad { get; set; } = string.Empty;
}

public class MalzemeInput
{
    [Required]
    public int TesisatGrubuId { get; set; }

    [Required]
    public int IsDetayiId { get; set; }

    [Required]
    [Display(Name = "Malzeme")]
    public string MalzemeAdi { get; set; } = string.Empty;

    [Required]
    public string Marka { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    [Display(Name = "Liste Fiyatı")]
    public decimal ListeFiyati { get; set; }

    [Range(0, 100)]
    [Display(Name = "İskonto (%)")]
    public decimal IskontoOrani { get; set; }
}

public class TeklifSatiriInput
{
    [Required]
    public int IsDetayiId { get; set; }

    [Required]
    public int MalzemeTanimId { get; set; }

    [Range(1, int.MaxValue)]
    public int Adet { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "İşçilik Birim Fiyat")]
    public decimal IscilikBirimFiyat { get; set; }
}

public class DashboardViewModel
{
    public List<TesisatGrubu> Gruplar { get; set; } = new();
    public TesisatGrubuInput Input { get; set; } = new();
}

public class TesisatDetayViewModel
{
    public TesisatGrubu Grup { get; set; } = new();
    public IsDetayiInput YeniIsDetayi { get; set; } = new();
}

public class IsDetayiViewModel
{
    public TesisatGrubu Grup { get; set; } = new();
    public IsDetayi IsDetayi { get; set; } = new();
    public TeklifSatiriInput YeniSatir { get; set; } = new();
    public List<MalzemeTanim> UygunMalzemeler { get; set; } = new();
}

public class MalzemeYonetimViewModel
{
    public List<TesisatGrubu> Gruplar { get; set; } = new();
    public List<MalzemeTanim> Malzemeler { get; set; } = new();
    public MalzemeInput Input { get; set; } = new();
}
