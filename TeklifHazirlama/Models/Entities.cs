namespace TeklifHazirlama.Models;

public class TesisatGrubu
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public List<IsDetayi> IsDetaylari { get; set; } = new();
    public decimal ToplamTutar => IsDetaylari.Sum(x => x.ToplamTutar);
}

public class IsDetayi
{
    public int Id { get; set; }
    public int TesisatGrubuId { get; set; }
    public string Ad { get; set; } = string.Empty;
    public List<TeklifSatiri> Satirlar { get; set; } = new();
    public decimal MalzemeToplam => Satirlar.Sum(x => x.MalzemeToplam);
    public decimal IscilikToplam => Satirlar.Sum(x => x.IscilikToplam);
    public decimal ToplamTutar => MalzemeToplam + IscilikToplam;
}

public class MalzemeTanim
{
    public int Id { get; set; }
    public int TesisatGrubuId { get; set; }
    public int IsDetayiId { get; set; }
    public string MalzemeAdi { get; set; } = string.Empty;
    public string Marka { get; set; } = string.Empty;
    public decimal ListeFiyati { get; set; }
    public decimal IskontoOrani { get; set; }
    public decimal BirimFiyat => ListeFiyati * (1 - IskontoOrani / 100);
}

public class TeklifSatiri
{
    public int Id { get; set; }
    public int MalzemeTanimId { get; set; }
    public int IsDetayiId { get; set; }
    public int Adet { get; set; }
    public decimal IscilikBirimFiyat { get; set; }
    public MalzemeTanim Malzeme { get; set; } = new();
    public decimal MalzemeToplam => Adet * Malzeme.BirimFiyat;
    public decimal IscilikToplam => Adet * IscilikBirimFiyat;
    public decimal Toplam => MalzemeToplam + IscilikToplam;
}
