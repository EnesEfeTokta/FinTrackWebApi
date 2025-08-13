# FinTrack Projesi: Sistem Mimarisi ve Teknik Dokümantasyon

Bu doküman, FinTrack platformunun teknik mimarisini, temel bileşenlerini, teknolojilerini, veri akışlarını ve aralarındaki etkileşimleri detaylı bir şekilde açıklamaktadır.

## 1. Genel Bakış ve Mimari Yaklaşım

FinTrack, **konteyner tabanlı (containerized)** ve **mikroservis odaklı** bir mimari üzerine inşa edilmiştir. Sistem, Docker ve Docker Compose kullanılarak yönetilen, birbirinden bağımsız ancak birbirleriyle API'ler üzerinden haberleşen servislerden oluşur. Bu yaklaşım, aşağıdaki avantajları sağlar:
*   **Ölçeklenebilirlik:** Her servis, ihtiyaç duyulduğunda bağımsız olarak ölçeklendirilebilir.
*   **Esneklik:** Her servis, kendi görevine en uygun teknoloji yığını ile geliştirilebilir (örn: .NET ve Python'un bir arada kullanılması).
*   **Bakım Kolaylığı:** Bir serviste yapılan değişiklik, diğer servisleri doğrudan etkilemez.
*   **Dağıtım Kolaylığı:** Tüm altyapı, Docker ile tek bir komutla ayağa kaldırılabilir.

## 2. Üst Düzey Mimari Diyagramı

Aşağıdaki diyagram, sistemin ana bileşenlerini, aralarındaki temel veri akışını ve dış dünya ile olan etkileşimini göstermektedir.

```mermaid
graph TD
    subgraph "Kullanıcılar & Dış Servisler"
        User["fa:fa-user Kullanıcı"]
        Admin["fa:fa-user-shield Operatör/Yönetici"]
        StripeSvc["fa:fa-stripe Stripe API"]
        EmailSvc["fa:fa-envelope SMTP Servisi"]
        CurrencyApi["fa:fa-globe Döviz Kuru API"]
    end

    subgraph "İstemci Uygulamaları"
        WPF_Client["fa:fa-windows FinTrack for Windows (WPF)"]
    end

    subgraph "Altyapı & Gateway"
        Nginx["fa:fa-server NGINX / API Gateway<br>Port: 80, 443"]
    end

    subgraph "Backend Servisleri (Docker Ağı: fintrac_network)"
        API_Main(<b>FinTrack Web API</b><br>.NET 8)
        API_Admin(<b>WinTrack Manager API</b><br>.NET 8)
        API_Bot(<b>FinBot Web API</b><br>Python & FastAPI)
        DB_Main["fa:fa-database MainDB - PostgreSQL"]
        DB_Log["fa:fa-database LogDB - PostgreSQL"]
        Ollama["fa:fa-brain Ollama & Mistral 7B"]
    end
    
    subgraph "Gözetim & DevOps (Monitoring & DevOps)"
        Prometheus["fa:fa-chart-line Prometheus"]
        Grafana["fa:fa-chart-bar Grafana"]
        PgBackup["fa:fa-save Veritabanı Yedekleme"]
    end

    User --> WPF_Client
    Admin --> API_Admin

    WPF_Client --> |HTTPS/REST API| Nginx
    
    Nginx --> |/api/*| API_Main
    Nginx --> |/admin-api/*| API_Admin

    API_Main <--> |REST API| API_Bot
    API_Main <--> |TCP/IP| DB_Main
    API_Main <--> |TCP/IP| DB_Log
    API_Main --> |API Çağrısı| EmailSvc
    API_Main --> |API Çağrısı| CurrencyApi
    API_Bot --> |Lokal Çağrı| Ollama

    StripeSvc --> |Webhook| API_Main

    API_Main -- "Metrikleri Topla" --> Prometheus
    API_Bot -- "Metrikleri Topla" --> Prometheus
    DB_Main -- "Metrikleri Topla" --> Prometheus
    
    Prometheus --> |Veri Sağla| Grafana
    Admin --> |Dashboard| Grafana
    
    PgBackup --> |Yedekle| DB_Main
```

## 3. Temel Bileşenler

### 3.1. Ana Backend Servisleri

*   **FinTrack Web API (`fintrack_api`):**
    *   **Teknoloji:** .NET 8, ASP.NET Core, Entity Framework Core.
    *   **Sorumluluklar:** Sistemin ana beynidir. Kullanıcı yetkilendirme (OTP, JWT), hesap/bütçe/işlem yönetimi, raporlama, Güvenli Borç Sistemi (GBS) iş mantığı, Stripe ödeme oturumu başlatma ve webhook dinleme gibi tüm temel işlevleri yürütür.
*   **FinBot Web API (`finbot_api`):**
    *   **Teknoloji:** Python, FastAPI.
    *   **Sorumluluklar:** Yapay zeka operasyonlarını yönetir. `FinTrack Web API`'sinden gelen kullanıcı sorgularını alır, `Ollama` servisi aracılığıyla Mistral 7B dil modeline iletir ve anlamlı yanıtlar üreterek geri döner.
*   **WinTrack Manager Panel (`wintrack_manager`):**
    *   **Teknoloji:** .NET 8, ASP.NET Core.
    *   **Sorumluluklar:** Yöneticiler ve operatörler için tasarlanmış bir API'dir. GBS'deki video onaylama/reddetme süreçleri, kullanıcı yönetimi, sistem genelindeki verileri izleme gibi idari fonksiyonları barındırır.

### 3.2. Veritabanı Mimarisi

*   **MainDB (`postgres_db`):**
    *   **Teknoloji:** PostgreSQL 15.
    *   **Sorumluluklar:** Ana uygulama verilerini (kullanıcılar, hesaplar, üyelikler, borçlar vb.) kalıcı olarak depolar.
*   **LogDB (`postgres_db_logs`):**
    *   **Teknoloji:** PostgreSQL 15.
    *   **Sorumluluklar:** Denetim (Audit) amacıyla kullanılır. `MainDB` üzerinde gerçekleşen her veri değişikliği (Ekleme, Güncelleme, Silme), kimin tarafından, ne zaman ve hangi verilerin değiştirildiği bilgisiyle bu veritabanına kaydedilir. Bu, ana veritabanının performansını korurken tam bir izlenebilirlik sağlar.

### 3.3. İstemci Uygulaması

*   **FinTrack for Windows (`WPF`):**
    *   **Teknoloji:** .NET, WPF, LiveCharts2.
    *   **Sorumluluklar:** Windows kullanıcıları için zengin ve yerel bir masaüstü deneyimi sunar. Kullanıcı etkileşimlerini alır ve bunları güvenli RESTful API çağrılarına dönüştürerek `FinTrack Web API`'sine iletir.

### 3.4. DevOps ve Gözetim (Monitoring)

*   **Docker & Docker Compose:** Tüm altyapıyı konteynerize eder ve yönetir.
*   **Prometheus:** Sistemdeki tüm servislerden (API'ler, veritabanları, konteynerler) anlık performans metriklerini toplar.
*   **Grafana:** Prometheus'tan gelen metrikleri, yöneticilerin sistemin genel sağlığını (CPU, RAM, API yanıt süreleri) bir bakışta görebileceği interaktif panolarda görselleştirir.
*   **Veritabanı Yedekleme (`postgres_backup_service`):** `MainDB`'yi her gece düzenli olarak otomatik olarak yedekler.

## 4. Güvenlik Mimarisi

*   **Kimlik Doğrulama:**
    *   **Kayıt:** E-posta sahipliğini doğrulamak için **OTP (One-Time Password)** sistemi kullanılır.
    *   **Giriş:** Başarılı giriş yapan kullanıcılara, rollerini ve izinlerini içeren, kısa ömürlü bir **JWT (JSON Web Token)** verilir.
*   **Yetkilendirme:** API endpoint'leri, `[Authorize(Roles = "User,Admin")]` gibi attribute'lar ile korunur. Sisteme gelen her istekte JWT'nin geçerliliği ve rolü kontrol edilir.
*   **Webhook Güvenliği:** Stripe'tan gelen webhook isteklerinin gerçekten Stripe'tan geldiğini doğrulamak için **imza doğrulama (Signature Verification)** mekanizması kullanılır.
*   **GBS Kriptografisi:** Güvenli Borç Sistemi'ndeki video delilleri, her video için özel olarak üretilen bir anahtarla **AES** algoritması kullanılarak şifrelenir. Anahtar, sadece alacaklıya teslim edilir ve sistemde saklanmaz.

## 5. Detaylı Süreç Akışları (Sequence Diagrams)

<details>
<summary><b>Akış 1: Yeni Kullanıcı Kaydı (İki Aşamalı OTP)</b></summary>

```mermaid
sequenceDiagram
    participant User as Kullanıcı
    participant ClientApp as WPF Uygulaması
    participant API as FinTrack API
    participant DB as Veritabanı
    participant Email as SMTP Servisi

    User->>ClientApp: Kayıt bilgilerini girer
    ClientApp->>API: POST /UserAuth/initiate-registration
    API->>API: OTP oluşturur, hash'ler
    API->>DB: OTP'yi ve geçici bilgileri kaydeder
    API->>Email: OTP'yi e-posta ile gönder
    API-->>ClientApp: 200 OK (OTP Gönderildi)

    User->>ClientApp: E-postadaki OTP'yi girer
    ClientApp->>API: POST /UserAuth/verify-otp-and-register
    API->>DB: OTP'yi doğrular
    alt OTP Doğru
        API->>DB: Kalıcı kullanıcıyı oluşturur (IsVerified=true)
        API->>DB: Geçici OTP kaydını siler
        API-->>ClientApp: 200 OK (Kayıt Başarılı)
    else OTP Yanlış
        API-->>ClientApp: 400 Bad Request
    end
```
</details>

<details>
<summary><b>Akış 2: Güvenli Borç Sistemi (GBS) Başlangıcı</b></summary>

```mermaid
sequenceDiagram
    participant Lender as Alacaklı
    participant Borrower as Borçlu
    participant ClientApp as WPF Uygulaması
    participant API as FinTrack API
    participant AdminAPI as Yönetici API
    participant Operator as Operatör

    Lender->>ClientApp: Borç teklifi oluşturur (Miktar, Vade, Borçlu E-postası)
    ClientApp->>API: POST /Debt/create-debt-offer
    API-->>ClientApp: 200 OK (Teklif Oluşturuldu)
    API->>Borrower: (Bildirim/E-posta ile) Yeni borç teklifi

    Borrower->>ClientApp: Teklifi kabul eder
    ClientApp->>API: POST /Debt/respond-to-offer/{id} (accepted: true)
    API-->>ClientApp: 200 OK (Durum: Video Bekleniyor)

    Borrower->>ClientApp: Taahhüt videosunu yükler
    ClientApp->>API: POST /Videos/user-upload-video
    API-->>ClientApp: 200 OK (Durum: Operatör Onayı Bekleniyor)
    
    API->>Operator: (Yönetim Paneli'nde) Yeni onay bekleyen video
    Operator->>AdminAPI: POST /Videos/video-approve/{id}
    AdminAPI->>API: (İç Servis Çağrısı) Videoyu Şifrele, Borcu Aktive Et
    API->>Lender: (E-posta ile) Borcunuz aktifleşti. Şifreleme Anahtarınız: [KEY]
```
</details>

<details>
<summary><b>Akış 3: Üyelik Satın Alma (Stripe)</b></summary>

```mermaid
sequenceDiagram
    participant User as Kullanıcı
    participant ClientApp as WPF Uygulaması
    participant API as FinTrack API
    participant DB as Veritabanı
    participant Stripe as Stripe API

    User->>ClientApp: "Plus" planını seçer
    ClientApp->>API: POST /Membership/create-checkout-session
    API->>DB: Yeni üyelik oluştur (Durum: PendingPayment)
    API->>Stripe: Ödeme oturumu oluşturma isteği
    Stripe-->>API: Oturum ID'si ve URL'si
    API-->>ClientApp: Stripe ödeme URL'sini döndür

    ClientApp->>User: Stripe ödeme sayfasına yönlendirir
    User->>Stripe: Ödeme bilgilerini girer ve tamamlar
    
    Stripe-->>API: (Webhook) POST /api/stripe/webhook (checkout.session.completed)
    API->>API: Webhook imzasını doğrular
    API->>DB: Üyelik durumunu "Active" olarak günceller
    API->>DB: Ödeme kaydını "Succeeded" olarak günceller
    API->>User: (E-posta ile) Ödeme onayı ve fatura gönderir
```
</details>