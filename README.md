# FinansTakipApp API <!-- Projenizin adını buraya yazın -->

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/kullanici_adiniz/proje_adiniz/actions) <!-- CI/CD kullanıyorsanız güncelleyin -->
[![Lisans](https://img.shields.io/badge/license-GPL-blue)](LICENSE) <!-- Lisans türünüze göre güncelleyin -->
[![.NET Versiyonu](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0) <!-- Kullandığınız .NET versiyonunu belirtin -->
[![Docker](https://img.shields.io/badge/docker-ready-blue)](https://www.docker.com/)

**Mikroservis Mimarisi ile Geliştirilmiş Finansal Takip Uygulaması**

Bu repository, FinTrack adlı finansal takip uygulamasının mikroservis mimarisi ile geliştirilmiş backend hizmetlerini içerir. Proje, ana API servisi (FinTrackWebApi), yönetim paneli (WinTrackManagerPanel) ve ChatBot servisi (FinBotWebApi) olmak üzere üç ana mikroservisten oluşmaktadır.

---

## İçindekiler

- Genel Bakış
- Mimari Yapı
- Özellikler
- Kullanılan Teknolojiler
- Ön Gereksinimler
- Kurulum
- Docker ile Çalıştırma
- Yapılandırma
- API Kullanımı ve Uç Noktalar (Endpoints)
- Testleri Çalıştırma
- Katkıda Bulunma
- Lisans
- İletişim

---

## Genel Bakış

FinTrack, modern mikroservis mimarisi kullanılarak geliştirilmiş, kullanıcıların kişisel finanslarını etkin bir şekilde yönetmelerine olanak tanıyan kapsamlı bir finansal takip ve yönetim platformudur. Sistem, bütçe oluşturma, gelir-gider takibi, hesap yönetimi, güncel kur bilgileri ve ChatBot destekli etkileşim gibi zengin özellikler sunar.

---

## Mimari Yapı

Proje, aşağıdaki mikroservislerden oluşmaktadır:

1. **FinTrackWebApi (Ana API Servisi)**
   - Kullanıcı yönetimi
   - Hesap ve işlem yönetimi
   - Bütçe yönetimi
   - Raporlama servisleri

2. **WinTrackManagerPanel (Yönetim Paneli)**
   - Sistem yönetimi
   - Kullanıcı yönetimi
   - İçerik moderasyonu
   - Sistem izleme ve raporlama

3. **FinBotWebApi (ChatBot Servisi)**
   - Kullanıcı destek sistemi
   - Finansal tavsiyeler
   - Otomatik yanıt sistemi

Her mikroservis kendi veritabanına sahiptir ve Docker konteynerleri içinde çalışmaktadır.

---

## Özellikler

✨ **Temel Özellikler:**

*   **Kullanıcı Yönetimi:** Kayıt olma, giriş yapma, profil yönetimi (JWT tabanlı kimlik doğrulama).
*   **Hesap Yönetimi:** Banka hesapları, kredi kartları, nakit vb. hesap tanımlama ve yönetimi.
*   **İşlem Yönetimi:** Gelir ve gider kayıtları ekleme, düzenleme, silme ve listeleme.
*   **Kategorizasyon:** İşlemleri özel veya ön tanımlı kategorilere ayırma.
*   **Bütçe Yönetimi:** Belirli kategoriler veya genel harcamalar için aylık/yıllık bütçe oluşturma ve takip etme.
*   **Raporlama:** Aylık özetler, kategori bazlı harcama analizleri, gelir-gider grafikleri.
*   **Para Birimi Desteği:** Farklı para birimleri ile işlem yapabilme.
*   **Güvenlik:** Güvenli kimlik doğrulama, yetkilendirme ve veri koruma mekanizmaları.
*   **ChatBot Desteği:** Finansal tavsiyeler ve kullanıcı desteği için yapay zeka destekli chatbot.
*   **Yönetim Paneli:** Kapsamlı sistem yönetimi ve izleme araçları.

---

## Kullanılan Teknolojiler

*   **Framework:** ASP.NET Core 8.0
*   **Dil:** C#
*   **Veritabanı:** PostgreSQL
*   **ORM:** Entity Framework Core 8.0
*   **API Dokümantasyonu:** Swagger
*   **Kimlik Doğrulama:** JWT (JSON Web Tokens)
*   **Mimari:** Mikroservis Mimarisi
*   **Containerization:** Docker
*   **Orchestration:** Docker Compose
*   **Dependency Injection:** .NET Core Dahili DI Container
*   **Logging:** .NET Core Dahili Logging
*   **Ödeme Sistemi:** Stripe
*   **Döviz Kuru API:** CurrencyFreaks
*   **ChatBot:** Python tabanlı özel chatbot servisi

---

## Ön Gereksinimler

Projeyi yerel makinenizde çalıştırmak veya geliştirmek için aşağıdaki araçların kurulu olması gerekmektedir:

*   [.NET SDK](https://dotnet.microsoft.com/download) (8.0 veya üzeri)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) / [Visual Studio Code](https://code.visualstudio.com/)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/)
*   [pgAdmin](https://www.pgadmin.org/)
*   [Git](https://git-scm.com/)
*   [Python](https://www.python.org/) (ChatBot servisi için)

---

## Kurulum

### Docker ile Kurulum (Önerilen)

1. **Repository'yi Klonlayın:**
    ```bash
    git clone https://github.com/kullanici_adiniz/proje_adiniz.git
    cd proje_adiniz
    ```

2. **Docker Compose ile Servisleri Başlatın:**
    ```bash
    docker-compose up -d
    ```

Bu komut, tüm mikroservisleri ve gerekli veritabanlarını otomatik olarak başlatacaktır.

### Manuel Kurulum

Her bir mikroservis için ayrı kurulum adımları:

1. **FinTrackWebApi Kurulumu:**
    ```bash
    cd FinTrackWebApi
    dotnet restore
    dotnet build
    dotnet run
    ```

2. **WinTrackManagerPanel Kurulumu:**
    ```bash
    cd WinTrackManagerPanel
    dotnet restore
    dotnet build
    dotnet run
    ```

3. **FinBotWebApi Kurulumu:**
    ```bash
    cd FinBotWebApi
    pip install -r requirements.txt
    python app.py
    ```

---

## Docker ile Çalıştırma

Proje, Docker ve Docker Compose kullanılarak kolayca çalıştırılabilir. Tüm servisler ve bağımlılıklar otomatik olarak yapılandırılır.

1. **Tüm Servisleri Başlatma:**
    ```bash
    docker-compose up -d
    ```

2. **Belirli Bir Servisi Başlatma:**
    ```bash
    docker-compose up -d fintrackwebapi
    ```

3. **Servisleri Durdurma:**
    ```bash
    docker-compose down
    ```

4. **Logları Görüntüleme:**
    ```bash
    docker-compose logs -f
    ```

---

## Yapılandırma

Her mikroservis kendi yapılandırma dosyalarına sahiptir. Temel yapılandırmalar `appsettings.json` ve ortama özel `appsettings.{Environment}.json` dosyaları üzerinden yapılır.

Örnek bir `appsettings.json` dosya yapısı:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=YOUR_DBNAME;Username=postgres;Password=YOUR_PASSWORD"
  },

  "Token": {
    "Issuer": "http://localhost:5246",
    "Audience": "http://localhost:5246",
    "SecurityKey": "YOUR_SECURITY_KEY",
    "Expiration": "YOUR_EXPIRATION"
  },

  "SMTP": {
    "NetworkCredentialMail": "YOUR_NETWORK_CREDENTIAL_MAIL",
    "NetworkCredentialPassword": "YOUR_NETWORK_CREDENTIAL_PASSWORD",
    "Host": "YOUR_HOST",
    "Port": 000,
    "SenderMail": "YOUR_SENDER_MAIL",
    "SenderName": "FinTrack"
  },

  "StripeSettings": {
    "PublishableKey": "YOUR_PUBLISHABLE_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "WebhookSecret": "YOUR_HOOK_SECRET",
    "FreeMembership": "YOUR_FREE_MEMBERSHIP",
    "PlusMembership": "YOUR_PLUS_MEMBERSHIP",
    "ProMembership": "YOUR_PRO_MEMBERSHIP"
  },

  "CurrencyFreaks": {
    "ApiKey": "YOUR_API_KEY",
    "BaseUrl": "https://api.currencyfreaks.com/v2.0/",
    "SupportedCurrenciesUrl": "https://api.currencyfreaks.com/v2.0/supported-currencies",
    "UpdateIntervalMinutes": 1440
  },

  "PythonChatBotService": {
    "Url": "http://finbotwebapi:8000/chat"
  },

  "FilePaths": {
    "UnapprovedVideos": "C:\\Path\\To\\UnapprovedVideos",
    "EncryptedVideos": "C:\\Path\\To\\EncryptedVideos"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Güvenlik Notu:** Üretim ortamı için hassas yapılandırma bilgilerini asla kaynak kod deposuna göndermeyin. Ortam Değişkenleri, Azure Key Vault, AWS Secrets Manager veya HashiCorp Vault gibi güvenli yapılandırma yönetimi çözümlerini kullanın.

---

## API Kullanımı ve Uç Noktalar (Endpoints)

Her mikroservis kendi API dokümantasyonuna sahiptir. Swagger UI üzerinden erişilebilir:

* **FinTrackWebApi:** `https://localhost:5246/swagger`
* **WinTrackManagerPanel:** `https://localhost:5247/swagger`
* **FinBotWebApi:** `https://localhost:8000/docs`

---

## Testleri Çalıştırma

Her mikroservis için testleri ayrı ayrı çalıştırabilirsiniz:

```bash
# FinTrackWebApi testleri
cd FinTrackWebApi
dotnet test

# WinTrackManagerPanel testleri
cd WinTrackManagerPanel
dotnet test

# FinBotWebApi testleri
cd FinBotWebApi
pytest
```

## Veri Tabanı

* Kullanılan PostgreSQL Sürümü: 15
* Veritabanı Şeması (ERD): Detaylı veritabanı şeması, tablolar, alanlar ve ilişkiler için lütfen [Veritabanı Şeması Detayları](./DATABASE.md) dokümanına bakınız.
* Veritabanı Migrasyonları: Entity Framework Core Migrations (`dotnet ef migrations add MigrationName`, `dotnet ef database update`).
* Veri Yedekleme ve Geri Yükleme Stratejisi:
    * Yedekleme: `pg_dump` aracı ile düzenli (günlük/haftalık) full ve incremental yedeklemeler. Yedeklerin güvenli bir depolama alanına (örn: S3 bucket, Azure Blob Storage) kopyalanması.
    * Geri Yükleme: `pg_restore` aracı ile. Düzenli olarak test geri yükleme senaryolarının uygulanması. Point-in-Time Recovery (PITR) yapılandırması.

---

## Katkıda Bulunma

1. Bu repository'yi fork edin
2. Yeni bir branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add some amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

---

## Lisans

Bu proje GPL lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

---

## İletişim

Proje ile ilgili sorularınız veya geri bildirimleriniz için:

*   **Proje Sahibi:** Enes Efe Tokta - [enesefetokta@gmail.com](mailto:enesefetokta@gmail.com)
*   **LinkedIn:** [https://www.linkedin.com/in/enes-efe-tokta/](https://www.linkedin.com/in/enes-efe-tokta/)
*   **Project Link:** [https://github.com/EnesEfeTokta/FluxNews](https://github.com/EnesEfeTokta/FinTrackWebApi)
