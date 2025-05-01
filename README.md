# FinansTakipApp API <!-- Projenizin adını buraya yazın -->

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/kullanici_adiniz/proje_adiniz/actions) <!-- CI/CD kullanıyorsanız güncelleyin -->
[![Lisans](https://img.shields.io/badge/license-GPL-blue)](LICENSE) <!-- Lisans türünüze göre güncelleyin -->
[![.NET Versiyonu](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0) <!-- Kullandığınız .NET versiyonunu belirtin -->

**ASP.NET Core Tabanlı Finansal Takip Uygulaması Web API'si**

Bu repository, FinTrack adlı finansal takip uygulamasının backend hizmetlerini sağlayan ASP.NET Core Web API projesini içerir. API, kullanıcıların gelirlerini, giderlerini yönetmelerine, bütçeler oluşturmalarına ve finansal raporlar almalarına olanak tanır.

---

## İçindekiler

- Genel Bakış
- Özellikler
- Kullanılan Teknolojiler
- Ön Gereksinimler
- Kurulum
- Yapılandırma
- API Kullanımı ve Uç Noktalar (Endpoints)
- Testleri Çalıştırma
- Katkıda Bulunma
- Lisans
- İletişim

---

## Genel Bakış

FinansTakipApp API, modern finansal yönetim ihtiyaçlarına cevap vermek üzere tasarlanmıştır. Güvenli, ölçeklenebilir ve bakımı kolay bir yapı sunmayı hedefler. RESTful prensiplerine uygun olarak tasarlanmış endpoint'ler aracılığıyla frontend uygulamaları (Web, Mobil vb.) veya diğer servisler ile entegre olabilir.

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

---

## Kullanılan Teknolojiler

*   **Framework:** ASP.NET Core 8.0
*   **Dil:** C#
*   **Veritabanı:** PostgreSQL
*   **ORM:** Entity Framework Core 8.0
*   **API Dokümantasyonu:** Swagger
*   **Kimlik Doğrulama:** JWT (JSON Web Tokens)
*   **Mimari:** Katmanlı Mimari
*   **Dependency Injection:** .NET Core Dahili DI Container
*   **Logging:** .NET Core Dahili Logging

---

## Ön Gereksinimler

Projeyi yerel makinenizde çalıştırmak veya geliştirmek için aşağıdaki araçların kurulu olması gerekmektedir:

*   [.NET SDK](https://dotnet.microsoft.com/download) (Projede belirtilen versiyon ile uyumlu)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) / [Visual Studio Code](https://code.visualstudio.com/)
*   [pgAdmin](https://www.pgadmin.org/)
*   [Git](https://git-scm.com/)

---

## Kurulum

Projeyi yerel makinenize kurmak için aşağıdaki adımları izleyin:

1.  **Repository'yi Klonlayın:**
    ```bash
    git clone https://github.com/kullanici_adiniz/proje_adiniz.git
    cd proje_adiniz/ApiProjeKlasoru <!-- Ana API proje klasörüne gidin -->
    ```

2.  **Bağımlılıkları Yükleyin:**
    ```bash
    dotnet restore
    ```

3.  **Veritabanı Bağlantısını Yapılandırın:**
    *   `appsettings.json` veya `appsettings.Development.json` dosyasındaki `ConnectionStrings` bölümünü kendi veritabanı sunucunuza göre güncelleyin.
    *   **Önemli:** Hassas bilgileri (şifre vb.) doğrudan `appsettings.json` dosyasına yazmak yerine [User Secrets](https://docs.microsoft.com/aspnet/core/security/app-secrets) veya Ortam Değişkenleri kullanmanız önerilir.
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=your_server_name;Database=FinansTakipDb;User Id=your_user;Password=your_password;TrustServerCertificate=True;" // Kendi bağlantı dizenizle değiştirin
      },
      // ... diğer ayarlar
    }
    ```

4.  **Veritabanı Migrasyonlarını Uygulayın (Entity Framework Core kullanılıyorsa):**
    *   Package Manager Console (Visual Studio) veya Terminal üzerinden:
    ```bash
    dotnet ef database update
    ```
    *   *Not: Migrasyonların bulunduğu projede bu komutu çalıştırmanız gerekebilir.*

5.  **Projeyi Derleyin:**
    ```bash
    dotnet build
    ```

6.  **Projeyi Çalıştırın:**
    ```bash
    dotnet run
    ```
    *   Alternatif olarak IDE üzerinden de projeyi başlatabilirsiniz (Genellikle F5 veya Yeşil Oynat butonu).

Uygulama başarıyla başlatıldığında, genellikle `https://localhost:XXXX` veya `http://localhost:YYYY` gibi bir adres üzerinden erişilebilir olacaktır. Swagger UI (`/swagger` endpoint'i) üzerinden API endpoint'lerini test edebilirsiniz.

---

## Yapılandırma

Uygulamanın temel yapılandırmaları `appsettings.json` ve ortama özel `appsettings.{Environment}.json` (örn. `appsettings.Development.json`) dosyaları üzerinden yapılır.

*   **Veritabanı Bağlantısı:** `ConnectionStrings` bölümünde tanımlanır.
*   **JWT Ayarları:** Token süresi, gizli anahtar (Secret Key), Issuer, Audience gibi ayarlar genellikle `JwtSettings` veya benzer bir bölümde bulunur. Gizli anahtarın güvenli bir şekilde saklandığından emin olun (User Secrets, Azure Key Vault vb.).
*   **Logging Ayarları:** Log seviyeleri ve hedefleri (Console, File, Seq vb.) yapılandırılır.
*   **Diğer Servis Ayarları:** Varsa dış servislerin URL'leri, API anahtarları vb.

**Güvenlik Notu:** Üretim ortamı için hassas yapılandırma bilgilerini asla kaynak kod deposuna göndermeyin. Ortam Değişkenleri, Azure Key Vault, AWS Secrets Manager veya HashiCorp Vault gibi güvenli yapılandırma yönetimi çözümlerini kullanın.

---

## API Kullanımı ve Uç Noktalar (Endpoints)

API, standart HTTP metotları (GET, POST, PUT, DELETE) ile çalışan RESTful endpoint'ler sunar. Tüm endpoint'ler ve detaylı istek/cevap formatları için proje çalıştırıldığında erişilebilen **Swagger UI** (`/swagger` adresi) arayüzünü kullanın.

**Örnek Endpoint'ler:**

*   `POST /api/auth/register`: Yeni kullanıcı kaydı.
*   `POST /api/auth/login`: Kullanıcı girişi ve JWT token alma.
*   `GET /api/accounts`: Kullanıcının tüm hesaplarını listeleme (Yetkilendirme gerektirir).
*   `POST /api/transactions`: Yeni bir finansal işlem ekleme (Yetkilendirme gerektirir).
*   `GET /api/transactions?accountId={id}`: Belirli bir hesaba ait işlemleri listeleme (Yetkilendirme gerektirir).
*   `GET /api/reports/monthly-summary?year=2024&month=07`: Belirtilen ay için finansal özet raporu (Yetkilendirme gerektirir).

**Kimlik Doğrulama:**
Güvenli endpoint'lere erişim için isteklerin `Authorization` başlığında geçerli bir `Bearer {JWT_TOKEN}` gönderilmesi gerekmektedir.

---

## Testleri Çalıştırma

Projede Unit Test ve/veya Integration Test'ler bulunuyorsa, aşağıdaki komut ile çalıştırabilirsiniz:

1.  Solution'ın kök dizinine gidin.
2.  Aşağıdaki komutu çalıştırın:
    ```bash
    dotnet test
    ```

## Katkıda Bulunma

Projeye katkıda bulunmak isterseniz, lütfen aşağıdaki adımları izleyin:

1.  Projeyi Fork'layın.
2.  Yeni bir Feature Branch oluşturun (`git checkout -b feature/YeniOzellik`).
3.  Değişikliklerinizi Commit'leyin (`git commit -m 'Yeni özellik eklendi'`).
4.  Fork'ladığınız Repository'ye Push'layın (`git push origin feature/YeniOzellik`).
5.  Bir Pull Request (PR) açın.

---

## Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına bakınız.

---

## İletişim

Proje ile ilgili sorularınız veya geri bildirimleriniz için:

*   **Proje Sahibi:** Enes Efe Tokta - [enesefetokta@gmail.com](mailto:enesefetokta@gmail.com)
*   **LinkedIn:** [https://www.linkedin.com/in/enes-efe-tokta/](https://www.linkedin.com/in/enes-efe-tokta/)
*   **Project Link:** [https://github.com/EnesEfeTokta/FluxNews](https://github.com/EnesEfeTokta/FinTrackWebApi)
