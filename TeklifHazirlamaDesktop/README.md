# Teklif Hazırlama Desktop (C# WinForms)

Bu klasör, mevcut web tabanlı sistemi C# Windows uygulaması olarak test etmek için başlangıç bir WinForms uygulaması içerir.

## Özellikler
- Teklif ekleme/silme
- Global Tesisat Grubu ve İş Detayı Grubu yönetimi
- Malzeme yönetimi (malzeme, marka, liste fiyatı, iskonto, işçilik)
- JSON dosyasına kalıcı kayıt (`teklif_hazirlama_data.json`, uygulama çalıştırılan dizin)

## Çalıştırma (Windows)
```bash
dotnet run --project TeklifHazirlamaDesktop/TeklifHazirlamaDesktop.csproj
```

> Not: Proje `net8.0-windows` + WinForms hedefler. Linux/macOS üzerinde çalıştırılamaz; Windows'ta test edilmelidir.
