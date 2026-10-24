# Logo Asistan Bot & Logo CRM Telegram Entegrasyonu

Logo ERP ve Logo CRM için geliştirilmiş web yönetim paneli, Telegram asistan botu, canlı finansal raporlama, e-belge (e-Fatura, e-İrsaliye, e-Arşiv) entegrasyonu ve yönetici özet raporlama sistemi.

## 🚀 Özellikler

* **Telegram Botu:** Plaka sorgulama, sipariş/teklif takibi, cari hesap ekstresi, çek raporları, canlı bildirimler ve sesli komut desteği.
* **Web Arayüzü & Dashboard:** `http://localhost:5000` üzerinden kullanıcı yönetimi, canlı hareket takibi, satış temsilcisi bazlı analizler ve sistem ayarları.
* **e-Belge Entegrasyonu:** e-Logo Connect Web Servisleri (e-Fatura, e-İrsaliye, e-Arşiv), GİB şematron doğrulama ve XSLT görüntüleme.
* **Yönetici Raporları:** Günlük otomatik yönetici özet raporları, borç yaşlandırma ve özel SQL rapor motoru.
* **Masaüstü Entegrasyonu:** Windows Sistem Tepsisi (Tray Icon) ve otomatik başlatma desteği.

## 🛠️ Teknoloji Yığını

* **Çalışma Zamanı:** .NET 10.0 (Windows Desktop & ASP.NET Core)
* **Veritabanı:** Microsoft SQL Server (Dapper & Microsoft.Data.SqlClient)
* **Telegram:** Telegram.Bot kütüphanesi
* **PDF Motoru:** QuestPDF & Select.HtmlToPdf
* **Mimari:** Clean Architecture (Domain, Application, Infrastructure, WebUI)

## 📦 Kurulum ve Çalıştırma

1. Projeyi klonlayın:
   ```bash
   git clone <REPO_URL>
   cd ActiveSource
   ```
2. `WebUI/appsettings.example.json` dosyasını `WebUI/appsettings.json` olarak kopyalayın ve kendi SQL / Telegram / e-Logo bilgilerinizi girin.
3. Projeyi derleyin ve çalıştırın:
   ```bash
   dotnet build LogoCrmTelegramIntegration.sln
   dotnet run --project WebUI/LogoCrmTelegramIntegration.WebUI.csproj
   ```
4. Web tarayıcınızdan `http://localhost:5000` adresine gidin.
