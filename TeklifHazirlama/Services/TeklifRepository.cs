using TeklifHazirlama.Models;

namespace TeklifHazirlama.Services;

public class TeklifRepository
{
    private readonly List<TesisatGrubu> _gruplar = new();
    private readonly List<MalzemeTanim> _malzemeler = new();
    private int _nextGrupId = 1;
    private int _nextIsDetayiId = 1;
    private int _nextMalzemeId = 1;
    private int _nextSatirId = 1;

    public List<TesisatGrubu> TumGruplar() => _gruplar.OrderBy(x => x.Ad).ToList();

    public TesisatGrubu? GrupGetir(int grupId) => _gruplar.FirstOrDefault(x => x.Id == grupId);

    public IsDetayi? IsDetayiGetir(int isDetayiId) => _gruplar.SelectMany(x => x.IsDetaylari).FirstOrDefault(x => x.Id == isDetayiId);

    public List<MalzemeTanim> TumMalzemeler() => _malzemeler.OrderBy(x => x.TesisatGrubuId).ThenBy(x => x.IsDetayiId).ThenBy(x => x.MalzemeAdi).ToList();

    public bool GrupEkle(string ad)
    {
        if (_gruplar.Any(x => x.Ad.Equals(ad, StringComparison.OrdinalIgnoreCase))) return false;
        _gruplar.Add(new TesisatGrubu { Id = _nextGrupId++, Ad = ad.Trim() });
        return true;
    }

    public bool GrupSil(int grupId)
    {
        var grup = GrupGetir(grupId);
        if (grup is null) return false;
        var isDetayiIds = grup.IsDetaylari.Select(x => x.Id).ToHashSet();
        _malzemeler.RemoveAll(x => x.TesisatGrubuId == grupId || isDetayiIds.Contains(x.IsDetayiId));
        _gruplar.Remove(grup);
        return true;
    }

    public bool IsDetayiEkle(int grupId, string ad)
    {
        var grup = GrupGetir(grupId);
        if (grup is null) return false;
        if (grup.IsDetaylari.Any(x => x.Ad.Equals(ad, StringComparison.OrdinalIgnoreCase))) return false;
        grup.IsDetaylari.Add(new IsDetayi { Id = _nextIsDetayiId++, TesisatGrubuId = grupId, Ad = ad.Trim() });
        return true;
    }

    public bool IsDetayiSil(int isDetayiId)
    {
        var grup = _gruplar.FirstOrDefault(x => x.IsDetaylari.Any(y => y.Id == isDetayiId));
        var hedef = grup?.IsDetaylari.FirstOrDefault(x => x.Id == isDetayiId);
        if (grup is null || hedef is null) return false;
        _malzemeler.RemoveAll(x => x.IsDetayiId == isDetayiId);
        grup.IsDetaylari.Remove(hedef);
        return true;
    }

    public string? MalzemeEkle(MalzemeInput input)
    {
        var grup = GrupGetir(input.TesisatGrubuId);
        var isDetayi = IsDetayiGetir(input.IsDetayiId);
        if (grup is null || isDetayi is null || isDetayi.TesisatGrubuId != grup.Id) return "Tesisat grubu ile iş detayı eşleşmiyor.";

        var ayniKayitVar = _malzemeler.Any(x =>
            x.IsDetayiId == input.IsDetayiId &&
            x.MalzemeAdi.Equals(input.MalzemeAdi.Trim(), StringComparison.OrdinalIgnoreCase) &&
            x.Marka.Equals(input.Marka.Trim(), StringComparison.OrdinalIgnoreCase));

        if (ayniKayitVar)
            return "Aynı iş detayında aynı malzeme + marka için ikinci kayıt eklenemez.";

        _malzemeler.Add(new MalzemeTanim
        {
            Id = _nextMalzemeId++,
            TesisatGrubuId = input.TesisatGrubuId,
            IsDetayiId = input.IsDetayiId,
            MalzemeAdi = input.MalzemeAdi.Trim(),
            Marka = input.Marka.Trim(),
            ListeFiyati = input.ListeFiyati,
            IskontoOrani = input.IskontoOrani
        });

        return null;
    }

    public bool MalzemeSil(int malzemeId)
    {
        var malzeme = _malzemeler.FirstOrDefault(x => x.Id == malzemeId);
        if (malzeme is null) return false;

        foreach (var satir in _gruplar.SelectMany(g => g.IsDetaylari).SelectMany(i => i.Satirlar).Where(s => s.MalzemeTanimId == malzemeId).ToList())
        {
            var isDetayi = IsDetayiGetir(satir.IsDetayiId);
            isDetayi?.Satirlar.Remove(satir);
        }

        _malzemeler.Remove(malzeme);
        return true;
    }

    public bool TeklifSatiriEkle(TeklifSatiriInput input)
    {
        var isDetayi = IsDetayiGetir(input.IsDetayiId);
        var malzeme = _malzemeler.FirstOrDefault(x => x.Id == input.MalzemeTanimId);
        if (isDetayi is null || malzeme is null || malzeme.IsDetayiId != isDetayi.Id) return false;

        isDetayi.Satirlar.Add(new TeklifSatiri
        {
            Id = _nextSatirId++,
            IsDetayiId = input.IsDetayiId,
            MalzemeTanimId = input.MalzemeTanimId,
            Adet = input.Adet,
            IscilikBirimFiyat = input.IscilikBirimFiyat,
            Malzeme = malzeme
        });

        return true;
    }

    public bool TeklifSatiriSil(int satirId)
    {
        var isDetayi = _gruplar.SelectMany(x => x.IsDetaylari).FirstOrDefault(x => x.Satirlar.Any(y => y.Id == satirId));
        var satir = isDetayi?.Satirlar.FirstOrDefault(x => x.Id == satirId);
        if (isDetayi is null || satir is null) return false;
        isDetayi.Satirlar.Remove(satir);
        return true;
    }

    public List<MalzemeTanim> IsDetayiMalzemeleri(int isDetayiId)
        => _malzemeler.Where(x => x.IsDetayiId == isDetayiId).OrderBy(x => x.MalzemeAdi).ThenBy(x => x.Marka).ToList();
}
