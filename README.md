### **FinTrack Projesi: Kapsamlı Teknik ve İşlevsel Dokümantasyon**

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/EnesEfeTokta/FinTrackWebApi)
[![Lisans](https://img.shields.io/badge/license-GPL-blue)](LICENSE)
[![.NET Versiyonu](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Python Versiyonu](https://img.shields.io/badge/Python-3.11-blue)](https://www.python.org/)
[![Docker](https://img.shields.io/badge/docker-ready-blue)](https://www.docker.com/)

**Mikroservis Mimarisi ile Geliştirilmiş Yeni Nesil Finansal Yönetim Platformu**

**Proje Sahibi:** [Adınız Soyadınız]
**Tarih:** 24.05.2024
**Versiyon:** 2.0

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
    *   7.3. Dinamik Kur Yönetim Sistemi
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

FinTrack, bireysel ve profesyonel kullanıcıların finansal hayatlarını tam kontrol altına almalarını sağlayan, yeni nesil bir **Hizmet Olarak Yazılım (SaaS)** platformudur. Mikroservis mimarisi üzerine inşa edilen proje, ana iş mantığını yürüten **FinTrackWebApi**, yapay zeka operasyonlarını yöneten **FinBotWebApi** ve sistemin genel idaresini sağlayan **WinTrackManagerPanel** servislerinden oluşur. Kapsamlı özellik seti, yapay zeka destekli akıllı asistanı ve çoklu platform desteği ile FinTrack, finansal yönetimde karmaşıklığı ortadan kaldırarak kullanıcılarına netlik, güvenlik ve verimlilik sunar.

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
*   **Veritabanı Motoru:** **PostgreSQL v15**
*   **ORM:** **Entity Framework (EF) Core 8.0**
*   **İkili Veritabanı Stratejisi:**
    *   **MainDB:** Ana uygulama verilerinin (kullanıcılar, hesaplar, işlemler vb.) tutulduğu birincil veritabanı.
    *   **LogDB:** MainDB üzerinde gerçekleşen tüm kritik veri manipülasyonu (POST, PUT, DELETE) işlemlerini denetim (audit) amacıyla kaydeden ikincil veritabanı.
*   **Yedekleme Stratejisi:** `pg_dump` aracı ile düzenli (günlük) yedeklemeler alınır ve güvenli bir depolama alanına kopyalanır. `Point-in-Time Recovery (PITR)` yapılandırması ile veri kaybı riski minimize edilir.

### **6. Kullanılan Teknolojiler**

| Kategori | Teknoloji / Araç |
| :--- | :--- |
| **Backend Framework** | ASP.NET Core 8.0, Python (FastAPI) |
| **Dil** | C# 12, Python 3.11 |
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
1.  **İstek:** Kullanıcı, e-posta, kullanıcı adı ve şifre bilgilerini `POST /api/Auth/user/initiate-registration` endpoint'ine gönderir.
2.  **İşlem:** Sunucu, bilgilerin benzersizliğini kontrol eder. 6 haneli bir OTP üretir. Bu OTP'nin hash'lenmiş halini ve kullanıcı bilgilerini (şifre dahil) geçici olarak `OtpVerifications` tablosuna kaydeder. Düz OTP'yi kullanıcıya e-posta ile gönderir.
3.  **Yanıt:** Başarılı olursa, "OTP e-posta adresinize gönderildi" mesajı döner.

##### **Adım B: OTP Doğrulama ve Kayıt Tamamlama**
1.  **İstek:** Kullanıcı, e-postasındaki kodu `POST /api/Auth/user/verify-otp-and-register` endpoint'ine gönderir.
2.  **İşlem:** Sunucu, OTP'yi doğrular. Doğruysa, geçici tablodan kullanıcı bilgilerini alır ve `UserManager.CreateAsync()` ile ASP.NET Identity sistemine kalıcı olarak kaydeder. Kullanıcıya varsayılan rol atanır ve geçici OTP kaydı silinir.
3.  **Yanıt:** "Kayıt başarılı. Artık giriş yapabilirsiniz" mesajı döner.

##### **Adım C: Kullanıcı Girişi ve Token Üretimi**
1.  **İstek:** Kullanıcı, e-posta ve şifresiyle `POST /api/Auth/user/login` endpoint'ine istek atar.
2.  **İşlem:** `SignInManager.CheckPasswordSignInAsync` ile kimlik bilgileri doğrulanır. Başarılıysa, kullanıcının ID, e-posta ve rollerini içeren bir **JWT Access Token** üretilir.
3.  **Yanıt:** `200 OK` ile birlikte kullanıcı bilgileri ve `accessToken` döndürülür. Bu token, korumalı endpoint'lere erişim için `Authorization: Bearer <token>` başlığında kullanılır.

#### **7.2. Güvenli Borç Sistemi (GBS)**
1.  **Teklif:** Borç veren, alacaklının e-postasını girerek borç teklifi gönderir.
2.  **Video Doğrulama:** Teklifi kabul eden borçlu, yasal taahhüt beyanı içeren bir **güvenlik videosu** çeker.
3.  **Operatör Onayı:** `WinTrackManagerPanel` üzerinden bir operatör, videoyu ve borç bilgilerini inceler.
4.  **Şifreleme:** Onaylanan video, geri döndürülemez şekilde şifrelenir. Videoyu açacak **20 karakterlik özel anahtar** sadece borç verene teslim edilir.
5.  **Otomatik Takip:** Vadesi geçen borçlarda, alacaklıya video erişim hakkı tanınır ve anahtarıyla videoyu deşifre edebilir.

#### **7.3. Dinamik Kur Yönetim Sistemi**
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
*   Python 3.11+

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
    "DefaultConnection": "Host=postgres_db;Port=5432;..."
  },
  "Token": {
    "SecurityKey": "YOUR_ULTRA_SECRET_KEY_FOR_JWT_SIGNING"
  },
  "StripeSettings": {
    "SecretKey": "sk_test_..."
  },
  "PythonChatBotService": {
    "Url": "http://finbot_api:8000/chat"
  }
}
```
**Güvenlik Notu:** Üretim ortamında hassas veriler için **ortam değişkenleri, Azure Key Vault** veya benzeri güvenli konfigürasyon yönetimi araçları kullanılmalıdır.

### **10. API Kullanımı ve Testler**

*   **API Dokümantasyonu:** Her servis, Swagger UI üzerinden kendi endpoint dokümantasyonunu sunar:
    *   **FinTrackWebApi:** `http://localhost:5246/swagger`
    *   **WinTrackManagerPanel:** `http://localhost:5247/swagger`
    *   **FinBotWebApi:** `http://localhost:8000/docs`
*   **Testleri Çalıştırma:**
    ```bash
    # .NET Projeleri için
    cd FinTrackWebApi
    dotnet test
    
    # Python Projesi için
    cd FinBotWebApi
    pytest
    ```

### **11. Sonuç ve Gelecek Vizyonu**

FinTrack, sadece bir finansal takip aracı değil, aynı zamanda kullanıcılarının finansal refahını artırmayı hedefleyen bütünsel bir ekosistemdir. Sağlam teknik temelleri, modern mikroservis mimarisi ve yenilikçi özellikleriyle pazarın ihtiyaçlarına cevap vermeye hazırdır. Gelecek vizyonu, **mobil uygulamalar (iOS & Android)**, **gelişmiş AI özellikleri** (anomali tespiti, proaktif bütçe optimizasyonu) ve **üçüncü parti entegrasyonlarını** içermektedir.

### **12. Lisans ve İletişim**

*   **Lisans:** Bu proje GPL lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.
*   **Proje Sahibi:** Enes Efe Tokta
*   **İletişim:** [enesefetokta@gmail.com](mailto:enesefetokta@gmail.com)
*   **LinkedIn:** [https://www.linkedin.com/in/enes-efe-tokta/](https://www.linkedin.com/in/enes-efe-tokta/)
*   **Proje Linki:** [https://github.com/EnesEfeTokta/FinTrackWebApi](https://github.com/EnesEfeTokta/FinTrackWebApi)