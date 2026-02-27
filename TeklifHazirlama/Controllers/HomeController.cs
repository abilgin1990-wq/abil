using Microsoft.AspNetCore.Mvc;
using TeklifHazirlama.Models;
using TeklifHazirlama.Services;

namespace TeklifHazirlama.Controllers;

public class HomeController : Controller
{
    private readonly TeklifRepository _repo;

    public HomeController(TeklifRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new DashboardViewModel { Gruplar = _repo.TumGruplar() });
    }

    [HttpPost]
    public IActionResult TesisatGrubuEkle(DashboardViewModel model)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Input.Ad))
        {
            TempData["Error"] = "Tesisat grubu adı zorunludur.";
            return RedirectToAction(nameof(Index));
        }

        if (!_repo.GrupEkle(model.Input.Ad))
            TempData["Error"] = "Aynı isimde tesisat grubu zaten mevcut.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult TesisatGrubuSil(int id)
    {
        if (!_repo.GrupSil(id))
            TempData["Error"] = "Silme işlemi başarısız.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult TesisatGrubu(int id)
    {
        var grup = _repo.GrupGetir(id);
        if (grup is null) return RedirectToAction(nameof(Index));

        return View(new TesisatDetayViewModel
        {
            Grup = grup,
            YeniIsDetayi = new IsDetayiInput { TesisatGrubuId = grup.Id }
        });
    }

    [HttpPost]
    public IActionResult IsDetayiEkle(IsDetayiInput input)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(input.Ad))
        {
            TempData["Error"] = "İş detayı adı zorunludur.";
            return RedirectToAction(nameof(TesisatGrubu), new { id = input.TesisatGrubuId });
        }

        if (!_repo.IsDetayiEkle(input.TesisatGrubuId, input.Ad))
            TempData["Error"] = "Aynı isimde iş detayı zaten mevcut.";

        return RedirectToAction(nameof(TesisatGrubu), new { id = input.TesisatGrubuId });
    }

    [HttpPost]
    public IActionResult IsDetayiSil(int id, int tesisatGrubuId)
    {
        if (!_repo.IsDetayiSil(id))
            TempData["Error"] = "İş detayı silinemedi.";

        return RedirectToAction(nameof(TesisatGrubu), new { id = tesisatGrubuId });
    }

    [HttpGet]
    public IActionResult IsDetayi(int id)
    {
        var isDetayi = _repo.IsDetayiGetir(id);
        if (isDetayi is null) return RedirectToAction(nameof(Index));

        var grup = _repo.GrupGetir(isDetayi.TesisatGrubuId);
        if (grup is null) return RedirectToAction(nameof(Index));

        return View(new IsDetayiViewModel
        {
            Grup = grup,
            IsDetayi = isDetayi,
            UygunMalzemeler = _repo.IsDetayiMalzemeleri(isDetayi.Id),
            YeniSatir = new TeklifSatiriInput { IsDetayiId = isDetayi.Id }
        });
    }

    [HttpPost]
    public IActionResult TeklifSatiriEkle(TeklifSatiriInput input)
    {
        var isDetayi = _repo.IsDetayiGetir(input.IsDetayiId);
        if (isDetayi is null) return RedirectToAction(nameof(Index));

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Satır bilgileri geçersiz.";
            return RedirectToAction(nameof(IsDetayi), new { id = input.IsDetayiId });
        }

        if (!_repo.TeklifSatiriEkle(input))
            TempData["Error"] = "Seçili malzeme ile satır eklenemedi. Malzeme yönetiminden kontrol ediniz.";

        return RedirectToAction(nameof(IsDetayi), new { id = input.IsDetayiId });
    }

    [HttpPost]
    public IActionResult TeklifSatiriSil(int satirId, int isDetayiId)
    {
        if (!_repo.TeklifSatiriSil(satirId))
            TempData["Error"] = "Satır silinemedi.";

        return RedirectToAction(nameof(IsDetayi), new { id = isDetayiId });
    }

    [HttpGet]
    public IActionResult MalzemeYonetimi()
    {
        return View(new MalzemeYonetimViewModel
        {
            Gruplar = _repo.TumGruplar(),
            Malzemeler = _repo.TumMalzemeler()
        });
    }

    [HttpPost]
    public IActionResult MalzemeEkle(MalzemeInput input)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Malzeme bilgilerini eksiksiz doldurun.";
            return RedirectToAction(nameof(MalzemeYonetimi));
        }

        var hata = _repo.MalzemeEkle(input);
        if (!string.IsNullOrEmpty(hata)) TempData["Error"] = hata;

        return RedirectToAction(nameof(MalzemeYonetimi));
    }

    [HttpPost]
    public IActionResult MalzemeSil(int id)
    {
        if (!_repo.MalzemeSil(id))
            TempData["Error"] = "Malzeme silinemedi.";

        return RedirectToAction(nameof(MalzemeYonetimi));
    }
}
