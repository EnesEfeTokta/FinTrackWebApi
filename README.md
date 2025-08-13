# **FinTrack Projesi: Kapsamlı Teknik ve İşlevsel Dokümantasyon**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/EnesEfeTokta/FinTrackWebApi)
[![Lisans](https://img.shields.io/badge/license-GPL-blue)](LICENSE)
[![.NET Versiyonu](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Python Versiyonu](https://img.shields.io/badge/Python-3.10-blue)](https://www.python.org/)
[![Docker](https://img.shields.io/badge/docker-ready-blue)](https://www.docker.com/)

**Mikroservis Mimarisi ile Geliştirilmiş Yeni Nesil Finansal Yönetim Platformu**

---

### **İçindekiler**

1.  **Yönetici Özeti**
2.  **Proje Vizyonu ve Hedef Kitle**
3.  **Temel Özellikler ve Yetenekler**
4.  **Abonelik Modelleri ve Gelir Stratejisi**
5.  **Sistem Mimarisi**
    *   5.1. Genel Mimari Bakış ve Mikroservis Yapısı
    *   5.2. Sunucu Tarafı (Backend) Mimarisi
    *   5.3. İstemci Tarafı (Frontend - WPF) Mimarisi
    *   5.4. Veritabanı Mimarisi ve Stratejisi
6.  **Kullanılan Teknolojiler**
7.  **Anahtar Sistemler ve İş Akışları**
    *   7.1. Kullanıcı Kimlik Doğrulama ve Kayıt (OTP & JWT)
    *   7.2. Güvenli Borç Sistemi (GBS)
    *   7.3. Çok Formatlı Raporlama Sistemi
    *   7.4. Dinamik Kur Yönetim Sistemi
8.  **DevOps, Konteynerizasyon ve Gözetim (Monitoring)**
    *   8.1. Docker Mimarisi ve Servisler
    *   8.2. Gözetim ve Sağlık Durumu İzleme (Prometheus & Grafana)
    *   8.3. Veri Yedekleme ve Güvenliği
9.  **Kurulum, Yapılandırma ve Çalıştırma**
    *   9.1. Ön Gereksinimler
    *   9.2. Docker ile Hızlı Kurulum (Önerilen)
    *   9.3. Yapılandırma (`appsettings.json`)
10. **API Kullanımı ve Testler**
11. **Sonuç ve Gelecek Vizyonu**
12. **Lisans ve İletişim**

---

### **1. Yönetici Özeti**

FinTrack, bireysel ve profesyonel kullanıcıların finansal hayatlarını tam kontrol altına almalarını sağlayan, yeni nesil bir **Hizmet Olarak Yazılım (SaaS)** platformudur. Mikroservis mimarisi üzerine inşa edilen proje, ana iş mantığını yürüten **FinTrackWebApi**, yapay zeka operasyonlarını yöneten **FinBotWebApi** ve sistemin genel idaresini sağlayan WinTrackManagerPanel(Geliştirme aşamasında...) servislerinden oluşur. Kapsamlı özellik seti, çok formatlı profesyonel raporlama yeteneği, yapay zeka destekli akıllı asistanı ve çoklu platform desteği ile FinTrack, finansal yönetimde karmaşıklığı ortadan kaldırarak kullanıcılarına netlik, güvenlik ve verimlilik sunar.

### **2. Proje Vizyonu ve Hedef Kitle**

**Vizyon:** Finansal okuryazarlığı artırmak ve her seviyeden kullanıcının finansal hedeflerine ulaşmasını kolaylaştıran, dünyanın en sezgisel ve güçlü finansal yönetim aracını oluşturmak.

**Hedef Kitle:**
*   **Giriş Seviyesi Kullanıcılar:** Kişisel harcamalarını takip etmek ve bütçe oluşturmak isteyenler.
*   **Orta ve İleri Seviye Kullanıcılar:** Birden fazla hesabı, yatırımı ve bütçeyi yöneten, detaylı analizlere ihtiyaç duyan bireyler ve aileler.
*   **Profesyoneller ve Serbest Çalışanlar:** Gelir-gider akışını titizlikle yöneten, detaylı raporlamaya ve yasal geçerliliği olan borç takibine ihtiyaç duyan profesyoneller.

### **3. Temel Özellikler ve Yetenekler**

*   **Kapsamlı Kur Desteği:** 950'den fazla küresel para birimini anlık olarak takip etme ve dönüştürme.
*   **Stratejik Bütçe Yönetimi:** Dinamik bütçeler oluşturma, harcama limitleri belirleme ve hedef takibi.
*   **Merkezi Hesap Takibi:** Tüm banka, kredi kartı ve yatırım hesaplarını tek panelden yönetme.
*   **Akıllı Gelir/Gider Analizi:** İşlemleri otomatik kategorize etme ve harcama alışkanlıkları hakkında içgörüler sunma.
*   **Güvenli Borç Sistemi (GBS):** Video doğrulamalı, şifreli ve yasal delil niteliği taşıyan kullanıcılar arası borç platformu.
*   **Yapay Zeka Destekli Finansal Asistan (FinBot):** Kişiselleştirilmiş finansal tavsiyeler sunan, soruları yanıtlayan ve proaktif çözümler üreten akıllı asistan.
*   **Detaylı ve Esnek Raporlama:** **PDF, WORD, TEXT, XML, EXCEL, MARKDOWN** formatlarında profesyonel raporlar.
*   **Kapsamlı Yönetim Paneli (WinTrackManagerPanel):** Sistem yönetimi, kullanıcı denetimi, içerik moderasyonu ve sistem sağlığı izleme.
*   **Veri Görselleştirme:** **LiveCharts2** ile interaktif grafikler sayesinde verileri anlaşılır içgörülere çevirme.

### **4. Abonelik Modelleri ve Gelir Stratejisi**

FinTrack, freemium modeliyle farklı kullanıcı segmentlerine hitap eder. Ödeme altyapısı, global standartlarda güvenlik sunan **Stripe** ile entegredir.

| Plan Adı | Fiyat | Hedef Kitle |
| :--- | :--- | :--- |
| **Free** | 0 USD/Ay | Temel finansal takip ve başlangıç seviyesi kullanıcılar. |
| **Plus** | 10 USD/Ay | Çoklu hesap ve bütçe yönetimi yapan orta seviye kullanıcılar. |
| **Pro** | 25 USD/Ay | Profesyoneller, serbest çalışanlar ve GBS gibi gelişmiş özelliklere ihtiyaç duyanlar. |

### **5. Sistem Mimarisi**

#### **5.1. Genel Mimari Bakış ve Mikroservis Yapısı**
FinTrack, birbirinden bağımsız, ölçeklenebilir ve esnek birimlerden oluşan **Mikroservis Mimarisi** üzerine inşa edilmiştir. Bu yapı, her servisin kendi teknoloji yığınını kullanmasına, bağımsız olarak geliştirilip dağıtılmasına olanak tanır.

1.  **FinTrackWebApi (Ana API Servisi):** Projenin kalbidir. Kullanıcı yönetimi, kimlik doğrulama, hesap/işlem/bütçe yönetimi, Stripe entegrasyonu ve raporlama gibi tüm kritik iş mantıklarını barındırır.
2.  **FinBotWebApi (ChatBot Servisi):** Yapay zeka operasyonlarını yönetir. **Ollama** ve **Mistral 7B** modeli ile entegrasyon için **Python & FastAPI** ile geliştirilmiştir.
3.  **WinTrackManagerPanel (Yönetim Paneli):** Sistem yöneticileri için tasarlanmış, kullanıcı yönetimi, GBS onay süreçleri, sistem izleme ve içerik moderasyonu gibi idari işlevleri sunan servistir.

#### **5.2. Sunucu Tarafı (Backend) Mimarisi**
*   **FinTrackWebApi & WinTrackManagerPanel:** **ASP.NET Core 8.0** üzerinde, **RESTful API** prensipleri ve **Dependency Injection (DI)** tasarım deseni ile geliştirilmiştir.
*   **FinBotWebApi:** **Python** ve yüksek performanslı **FastAPI** çatısı ile geliştirilmiştir.

#### **5.3. İstemci Tarafı (Frontend - WPF) Mimarisi**
*   **FinTrackForWindows (WPF):** Windows için zengin bir masaüstü deneyimi sunan yerel uygulamadır. FinTrackWebApi ile güvenli **REST API** çağrıları üzerinden haberleşir.
*   **Merkezi Veri Yönetimi (Store Pattern):** `AccountStore`, `BudgetStore` gibi merkezi servisler (Store'lar) kullanılarak veri yönetimi tekilleştirilir. Bu, bileşenler arası veri tutarlılığını sağlar ve API'ye yapılan çağrıları optimize eder.

#### **5.4. Veritabanı Mimarisi ve Stratejisi**

#### Veri Tabanı
*   **Veritabanı Motoru:** PostgreSQL 15
*   **ORM (Object-Relational Mapper):** Entity Framework Core 8
*   **Yaklaşım:** Code-First with Migrations

#### Veritabanı Mimarisi

FinTrack, veri bütünlüğünü ve denetlenebilirliği sağlamak için ikili bir veritabanı stratejisi kullanır:

1.  **MainDB:** Ana uygulama verilerinin (kullanıcılar, hesaplar, işlemler, bütçeler vb.) tutulduğu birincil veritabanıdır.
2.  **LogDB:** `MainDB` üzerinde gerçekleşen tüm veri manipülasyonu (POST, PUT, DELETE) işlemlerini denetim (audit) amacıyla kaydeden ikincil veritabanıdır.

#### Detaylı Veritabanı Şeması (ERD & Tablo Yapısı)

Veritabanının tüm tablolarını, kolonlarını, veri tiplerini, ilişkilerini (ilişki diyagramı dahil), kısıtlamalarını ve indekslerini içeren kapsamlı teknik dokümantasyon için lütfen aşağıdaki dosyaya göz atın:

➡️ **[Detaylı Veritabanı Şeması Dokümanı](./docs/DATABASE.md)**

### **6. Kullanılan Teknolojiler**

| Kategori | Teknoloji / Araç |
| :--- | :--- |
| **Backend Framework** | ASP.NET Core 8.0, Python (FastAPI) |
| **Dil** | C# 12, Python 3.10-slim |
| **Frontend** | WPF (.NET) |
| **Veritabanı** | PostgreSQL v15 |
| **ORM** | Entity Framework Core 8.0 |
| **Mimari** | Mikroservis Mimarisi, RESTful API |
| **Konteynerizasyon** | Docker, Docker Compose |
| **Kimlik Doğrulama** | JWT (JSON Web Tokens), ASP.NET Core Identity, OTP |
| **Gözetim (Monitoring)** | Prometheus, Grafana, cAdvisor, Node Exporter |
| **Ödeme Sistemi** | Stripe SDK |
| **AI/ChatBot** | Ollama, Mistral 7B |
| **API Dokümantasyonu** | Swagger (OpenAPI) |
| **Bildirimler** | SMTP, Notification.Wpf |
| **Görselleştirme** | LiveCharts2 |

### **7. Anahtar Sistemler ve İş Akışları**

#### **7.1. Kullanıcı Kimlik Doğrulama ve Kayıt (OTP & JWT)**

##### **Adım A: Kayıt Başlatma ve OTP Gönderimi**
1.  **İstek:** Kullanıcı, e-posta, kullanıcı adı ve şifre bilgilerini `POST /Auth/user/initiate-registration` endpoint'ine gönderir.
2.  **İşlem:** Sunucu, bilgilerin benzersizliğini kontrol eder. 6 haneli bir OTP üretir. Bu OTP'nin hash'lenmiş halini ve kullanıcı bilgilerini (şifre dahil) geçici olarak `OtpVerifications` tablosuna kaydeder. Düz OTP'yi kullanıcıya e-posta ile gönderir.
3.  **Yanıt:** Başarılı olursa, "OTP e-posta adresinize gönderildi" mesajı döner.

##### **Adım B: OTP Doğrulama ve Kayıt Tamamlama**
1.  **İstek:** Kullanıcı, e-postasındaki kodu `POST /Auth/user/verify-otp-and-register` endpoint'ine gönderir.
2.  **İşlem:** Sunucu, OTP'yi doğrular. Doğruysa, geçici tablodan kullanıcı bilgilerini alır ve `UserManager.CreateAsync()` ile ASP.NET Identity sistemine kalıcı olarak kaydeder. Kullanıcıya varsayılan rol atanır ve geçici OTP kaydı silinir.
3.  **Yanıt:** "Kayıt başarılı. Artık giriş yapabilirsiniz" mesajı döner.

##### **Adım C: Kullanıcı Girişi ve Token Üretimi**
1.  **İstek:** Kullanıcı, e-posta ve şifresiyle `POST /Auth/user/login` endpoint'ine istek atar.
2.  **İşlem:** `SignInManager.CheckPasswordSignInAsync` ile kimlik bilgileri doğrulanır. Başarılıysa, kullanıcının ID, e-posta ve rollerini içeren bir **JWT Access Token** üretilir.
3.  **Yanıt:** `200 OK` ile birlikte kullanıcı bilgileri ve `accessToken` döndürülür. Bu token, korumalı endpoint'lere erişim için `Authorization: Bearer <token>` başlığında kullanılır.

#### **7.2. Güvenli Borç Sistemi (GBS)**
1.  **Teklif:** Borç veren, alacaklının e-postasını girerek borç teklifi gönderir.
2.  **Video Doğrulama:** Teklifi kabul eden borçlu, yasal taahhüt beyanı içeren bir **güvenlik videosu** çeker.
3.  **Operatör Onayı:** `WinTrackManagerPanel` üzerinden bir operatör, videoyu ve borç bilgilerini inceler.
4.  **Şifreleme:** Onaylanan video, geri döndürülemez şekilde şifrelenir. Videoyu açacak **20 karakterlik özel anahtar** sadece borç verene teslim edilir.
5.  **Otomatik Takip:** Vadesi geçen borçlarda, alacaklıya video erişim hakkı tanınır ve anahtarıyla videoyu deşifre edebilir.

#### **7.3. Çok Formatlı Raporlama Sistemi**
Kullanıcıların finansal verilerini anlamlı ve taşınabilir belgelere dönüştürmesini sağlayan esnek bir sistemdir.
1.  **İstek:** Kullanıcı, WPF uygulaması üzerinden raporlamak istediği veri aralığını (tarih, hesaplar, kategoriler vb.) ve istediği formatı (PDF, WORD, EXCEL, XML, TEXT, MARKDOWN) seçer.
2.  **API Çağrısı:** İstemci, bu kriterleri içeren bir isteği FinTrackWebApi üzerindeki ilgili raporlama endpoint'ine (örn: POST /api/reports/generate) gönderir.
Veri Toplama ve İşleme: Sunucu, isteğe göre veritabanından ilgili finansal verileri çeker.
3.  **Rapor Üretimi:** Çekilen veriler, seçilen formata uygun kütüphaneler (örn: PDF için QuestPDF, Excel için ClosedXML) kullanılarak işlenir ve bir dosya akışına (stream) dönüştürülür.
4.  **Yanıt:** Oluşturulan dosya akışı, uygun Content-Type başlığı ile istemciye geri döndürülür. İstemci de bu dosyayı kullanıcıya indirilebilir olarak sunar.

#### **7.4. Dinamik Kur Yönetim Sistemi**
Veri depolama verimliliği için akıllı bir mekanizma kullanılır. Sistem, dış sağlayıcıdan çektiği kur verisini mevcut veri ile karşılaştırır. Eğer kurdaki değişiklik anlamlı bir seviyenin (örn: ondalık 6. basamak) altındaysa, yeni kayıt oluşturmak yerine mevcut kaydın bir "çarpan" değeri güncellenir. Bu, veritabanı boyutunu optimize eder ve sorgu performansını artırır.

### **8. DevOps, Konteynerizasyon ve Gözetim (Monitoring)**

#### **8.1. Docker Mimarisi ve Servisler**
Tüm sistem bileşenleri, **Docker** ile konteynerize edilerek taşınabilirlik, izolasyon ve kolay dağıtım sağlanmıştır.

| Servis Adı | Teknoloji/Amaç | Açıklama |
| :--- | :--- | :--- |
| **fintrack_api** | ASP.NET Core | Ana iş mantığını yürüten Web API. |
| **finbot_api** | Python/FastAPI | Yapay zeka asistanı servisi. |
| **wintrack_manager**| ASP.NET Core | Yönetim paneli servisi. |
| **postgres_db** | PostgreSQL | Ana uygulama veritabanı (MainDB). |
| **postgres_db_logs**| PostgreSQL | Loglama veritabanı (LogDB). |
| **Ollama** | AI Engine | Mistral 7B modelini çalıştıran yapay zeka motoru. |
| **Prometheus** | Monitoring | Metrikleri toplayan zaman serisi veritabanı. |
| **Grafana** | Visualization | Metrikleri görselleştiren dashboard. |
| **node_exporter** | Exporter | API servislerinden sağlık metrikleri toplar. |
| **postgres_exporter**| Exporter | PostgreSQL'den sağlık metrikleri toplar. |
| **cAdvisor** | Monitoring | Docker konteynerlerinin performans metriklerini toplar. |
| **postgres_backup**| Backup | MainDB'yi her gece 03:00'da otomatik yedekler. |
| **ngrok** | Tunneling | Geliştirme API'sini internete açar. |

#### **8.2. Gözetim ve Sağlık Durumu İzleme (Prometheus & Grafana)**
*   **Prometheus:** `node_exporter`, `postgres_exporter` ve `cAdvisor` aracılığıyla tüm sistem bileşenlerinden (API'ler, veritabanları, Docker) anlık performans metriklerini toplar.
*   **Grafana:** Prometheus'ta toplanan verileri, sistemin genel sağlığını (CPU, RAM, API yanıt süreleri vb.) gösteren interaktif **dashboard**'larda görselleştirir.

#### **8.3. Veri Yedekleme ve Güvenliği**
`postgres_backup_service` konteyneri, her gece 03:00'te `MainDB`'nin tam yedeğini alarak olası bir veri kaybına karşı sistemi güvence altına alır.

### **9. Kurulum, Yapılandırma ve Çalıştırma**

#### **9.1. Ön Gereksinimler**
*   .NET SDK 8.0+
*   Docker Desktop
*   Git
*   Python 3.10-Slim

#### **9.2. Docker ile Hızlı Kurulum (Önerilen)**
1.  **Repository'yi Klonlayın:**
    ```bash
    git clone https://github.com/EnesEfeTokta/FinTrackWebApi.git
    cd FinTrackWebApi
    ```
2.  **Docker Compose ile Servisleri Başlatın:**
    ```bash
    docker-compose up -d
    ```
    Bu komut, tüm mikroservisleri, veritabanlarını ve bağımlılıkları otomatik olarak başlatacaktır.

#### **9.3. Yapılandırma (`appsettings.json`)**
Hassas yapılandırmalar (API anahtarları, şifreler) `appsettings.json` dosyası ve ortam değişkenleri ile yönetilir.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=Your_Port;Database=Your_DB;Username=Your_UserName;Password=Your_Password",
    "LogConnection": "Host=localhost;Port=Your_Port;Database=Your_DB;Username=Your_UserName;Password=Your_Password"
  },

  "Token": {
    "Issuer": "Your_Url",
    "Audience": "Your_Url",
    "SecurityKey": "Your_SecurityKey",
    "Expiration": 0
  },

  "SMTP": {
    "NetworkCredentialMail": "Your_Email",
    "NetworkCredentialPassword": "Your_Password",
    "Host": "Your_Host",
    "Port": "Your_Port",
    "SenderMail": "Your_Email",
    "SenderName": "FinTrack"
  },

  "StripeSettings": {
    "PublishableKey": "Your_PublishableKey",
    "SecretKey": "Your_SecretKey",
    "WebhookSecret": "Your_WebhookSecret",
    "FreeMembership": null,
    "PlusMembership": null,
    "ProMembership": null
  },

  "CurrencyFreaks": {
    "ApiKey": "Your_ApiKey",
    "BaseUrl": "https://api.currencyfreaks.com/v2.0/",
    "SupportedCurrenciesUrl": "https://api.currencyfreaks.com/v2.0/supported-currencies",
    "UpdateIntervalMinutes": 0
  },

  "PythonChatBotService": {
    "Url": "http://localhost:8000/chat"
  },

  "FilePaths": {
    "UnapprovedVideos": "videos/unapproved",
    "EncryptedVideos": "videos/encrypted"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "Microsoft.EntityFrameworkCore.Infrastructure": "Information",
      "Microsoft.EntityFrameworkCore.Model.Validation": "Information",
      "Microsoft.AspNetCore.Authentication": "Debug",
      "Microsoft.AspNetCore.Authorization": "Debug"

    }
  },
  "AllowedHosts": "*"
}
```
**Güvenlik Notu:** Üretim ortamında hassas veriler için **ortam değişkenleri, Azure Key Vault** veya benzeri güvenli konfigürasyon yönetimi araçları kullanılmalıdır.

### **10. API Kullanımı**

*   **API Referansı ve Detaylı Dokümantasyon**
FinTrack API'si, interaktif olarak test edilebileceği Swagger arayüzü ve her bir controller için hazırlanmış detaylı Markdown dokümanları ile belgelenmiştir.

*   **İnteraktif API Dokümantasyonu (Swagger)**
Canlı olarak API endpoint'lerini test etmek ve şemaları görmek için, proje çalıştırıldığında aşağıdaki adresten Swagger arayüzüne erişebilirsiniz:
    * FinTrackWebApi: http://localhost:5246/swagger

*  **Grafana**
Sistemin genel sağlığını tek bir merkezi yerden incelememize olanak sağlıyor. Varsayılan olarak kullanıcı adı Admin iken şifre de Admin 'dir.
    * Grafana: http://localhost:3000

*  **Detaylı Endpoint Dokümanları**
Aşağıda, **FinTrackWebApi** servisinin her bir controller'ı için hazırlanmış detaylı dokümantasyon dosyalarına linkler bulunmaktadır. Her doküman, endpoint'in amacını, gerekli istek formatlarını, başarılı ve hata yanıtlarını detaylı bir şekilde açıklamaktadır.

    * [**`UserAuthController`**](./docs/api/UserAuthController.md) - Kullanıcı kayıt, OTP doğrulama ve giriş işlemlerini yönetir.
    *  [**`UserController`**](./docs/api/UserController.md) - Giriş yapmış kullanıcının tüm profil, ayar ve özet verilerini tek bir merkezden sunar.
    * [**`UserSettingsController`**](./docs/api/UserSettingsController.md) - Kullanıcının profil, güvenlik, uygulama ve bildirim ayarlarını yönetir.
    * [**`AccountController`**](./docs/api/AccountController.md) - Kullanıcının finansal hesaplarını (banka, nakit vb.) yönetir.
    * [**`TransactionCategoryController`**](./docs/api/TransactionCategoryController.md) - Gelir/Gider işlemleri için kişisel kategorileri yönetir.
    * [**`TransactionsController`**](./docs/api/TransactionsController.md) - Tüm gelir/gider işlemlerini kaydeder ve filtreler.
    * [**`BudgetsController`**](./docs/api/BudgetsController.md) - Kullanıcının bütçelerini oluşturur ve yönetir.
    * [**`ReportsController`**](./docs/api/ReportsController.md) - Çeşitli formatlarda (PDF, Excel vb.) dinamik finansal raporlar üretir.
    * [**`DebtController`**](./docs/api/DebtController.md) - Güvenli Borç Sistemi'nin (GBS) ana iş akışını yönetir (teklif, kabul, temerrüt).
    * [**`VideosController`**](./docs/api/VideosController.md) - GBS için video yükleme, şifreleme ve güvenli izleme süreçlerini yönetir.
    * [**`MembershipController`**](./docs/api/MembershipController.md) - Abonelik planlarını ve kullanıcı üyeliklerini yönetir, Stripe ödeme oturumlarını başlatır.
    * [**`StripeWebhookController`**](./docs/api/StripeWebhookController.md) - Stripe'tan gelen başarılı ödeme olaylarını dinler ve üyelikleri otomatik olarak aktive eder.
    * [**`NotificationController`**](./docs/api/NotificationController.md) - Kullanıcıya özel uygulama içi bildirimleri yönetir.
    * [**`FeedbackController`**](./docs/api/FeedbackController.md) - Kullanıcıların geri bildirim göndermesini sağlar.
    * [**`ChatController`**](./docs/api/ChatController.md) - FinBot (Python) servisi için bir proxy görevi görerek güvenli iletişimi sağlar.
    * [**`LogController`**](./docs/api/LogController.md) - Sistem log dosyalarına (güvenlikli) erişim sağlar.

### **Proje Dokümantasyonu**
Bu proje, farklı teknik seviyelere hitap eden kapsamlı dokümanlarla desteklenmektedir. İhtiyacınız olan bilgiye hızlıca ulaşmak için aşağıdaki linkleri kullanabilirsiniz.

*   ➡️ **[Sistem Mimarisi](./docs/ARCHITECTURE.md)**
    *   Projenin üst düzey mimarisini, servislerini, teknolojilerini ve aralarındaki veri akışını anlamak için bu dokümanı okuyun.

*   ➡️ **[Detaylı Veritabanı Şeması](./docs/DATABASE.md)**
    *   Veritabanı tablolarının, kolonlarının, ilişkilerinin ve ERD'nin detaylı bir açıklaması için buraya bakın.

*   ➡️ **[API Referansı ve Endpoint Detayları](./docs/api/)**
    *   Tüm API endpoint'lerinin teknik detaylarını, istek/yanıt formatlarını ve kullanım örneklerini içeren Markdown dosyaları burada toplanmıştır.

*   ➡️ **[Sıkça Sorulan Sorular (SSS)](./docs/FAQ.md)**
    *   Projeyi kurma, çalıştırma, API kullanımı ve sık karşılaşılan sorunlar hakkında hızlı yanıtlar için bu dokümanı inceleyin.

*   ➡️ **[Roller ve Yetki Matrisi](./docs/ROLES_AND_PERMISSIONS.md)**
    *   Sistemdeki kullanıcı rollerini (`User`, `Admin` vb.) ve bu rollerin API üzerindeki yetkilerini anlamak için bu dokümanı kullanın.

### **11. Sonuç ve Gelecek Vizyonu**

FinTrack, sadece bir finansal takip aracı değil, aynı zamanda kullanıcılarının finansal refahını artırmayı hedefleyen bütünsel bir ekosistemdir. Sağlam teknik temelleri, modern mikroservis mimarisi ve yenilikçi özellikleriyle pazarın ihtiyaçlarına cevap vermeye hazırdır. Gelecek vizyonu, **mobil uygulamalar (iOS & Android)**, **gelişmiş AI özellikleri** (anomali tespiti, proaktif bütçe optimizasyonu) ve **üçüncü parti entegrasyonlarını** içermektedir.

### **12. Lisans ve İletişim**

*   **Lisans:** Bu proje GPL lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.
*   **Proje Sahibi:** Enes Efe Tokta
*   **İletişim:** [enesefetokta@gmail.com](mailto:enesefetokta@gmail.com)
*   **LinkedIn:** [https://www.linkedin.com/in/enes-efe-tokta/](https://www.linkedin.com/in/enes-efe-tokta/)
*   **Proje Linki:** [https://github.com/EnesEfeTokta/FinTrackWebApi](https://github.com/EnesEfeTokta/FinTrackWebApi)