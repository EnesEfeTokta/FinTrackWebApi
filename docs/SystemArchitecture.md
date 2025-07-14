# FinTrack Projesi Sistem Mimarisi

Bu doküman, FinTrack platformunun teknik mimarisi, temel bileşenleri, teknolojileri ve aralarındaki etkileşimler hakkında üst düzey bir genel bakış sunmaktadır.

## 1. Genel Bakış ve Mimari Yaklaşım

FinTrack, kişisel finans yönetimi için tasarlanmış, **konteyner tabanlı (containerized)** ve **mikroservis odaklı** bir mimari üzerine inşa edilmiştir. Sistem, Docker ve Docker Compose kullanılarak yönetilen, birbirinden bağımsız ancak birbirleriyle iletişim halinde olan servislerden oluşur. Bu yaklaşım, geliştirme ortamının standartlaştırılmasını, dağıtımın kolaylaştırılmasını ve sistemin ölçeklenebilirliğini artırmayı hedefler.

## 2. Üst Düzey Mimari Diyagramı

Aşağıdaki diyagram, sistemin ana bileşenlerini ve aralarındaki temel veri akışını göstermektedir.

```mermaid
graph TD
    subgraph "Kullanıcılar"
        User[<i class="fa fa-user"></i> Kullanıcı]
    end

    subgraph "İstemci Uygulamaları"
        Client_Web[<i class="fa fa-window-maximize"></i> Web Uygulaması]
        Client_Mobile[<i class="fa fa-mobile-alt"></i> Mobil Uygulama]
    end

    subgraph "Backend Servisleri (Docker Ağı)"
        API_Main(<b>FinTrack Web API</b><br>.NET 8)
        API_Bot(<b>FinBot Web API</b><br>Python 3.10)
        DB[(<i class="fa fa-database"></i> PostgreSQL DB)]
    end

    subgraph "Harici Servisler"
        EmailSvc[<i class="fa fa-envelope"></i> E-posta/SMS Servisi]
    end

    User --> Client_Web
    User --> Client_Mobile

    Client_Web --> |HTTPS/REST API| API_Main
    Client_Mobile --> |HTTPS/REST API| API_Main

    API_Main <--> |REST API| API_Bot
    API_Main <--> |TCP/IP| DB
    API_Main --> |API Çağrısı| EmailSvc
```

## 3. Temel Bileşenler

### 3.1. FinTrack Web API (`fintrackwebapi`)
- **Teknoloji:** .NET 8, ASP.NET Core, Entity Framework Core
- **Sorumluluklar:**
    - Sistemin ana beyni ve giriş kapısıdır.
    - Kullanıcı yetkilendirme (kayıt, giriş, JWT oluşturma) ve yönetimi.
    - Tüm temel CRUD (Oluştur, Oku, Güncelle, Sil) işlemleri (Hesaplar, Kategoriler, İşlemler, Bütçeler).
    - Veritabanı ile doğrudan iletişim.
    - Gerekli durumlarda `FinBot Web API`'sine istek göndermek.
    - OTP gönderme gibi işlemler için harici e-posta servislerini tetiklemek.

### 3.2. FinBot Web API (`finbotwebapi`)
- **Teknoloji:** Python 3.10, FastAPI
- **Sorumluluklar:**
    - Ana API'nin yükünü hafifleten yardımcı ve asenkron görevleri yürütür.
    - Kategori önerileri, harcama analizi raporları oluşturma veya yapay zeka tabanlı işlemler gibi özellikler için tasarlanmıştır.
    - `FinTrack Web API`'sinden gelen istekleri işler.

### 3.3. PostgreSQL Veritabanı (`db_postgres`)
- **Teknoloji:** PostgreSQL 15
- **Sorumluluklar:**
    - Uygulamanın tüm ilişkisel verilerini (kullanıcılar, hesaplar, işlemler vb.) kalıcı olarak depolar.
    - `docker-compose.yml` dosyasında tanımlanan bir Docker volume'ü (`postgres_data`) sayesinde verilerin konteyner durdurulsa bile korunmasını sağlar.

### 3.4. İstemci Uygulamaları (Client Applications)
- **Teknoloji:** Web için React/Angular/Vue, Mobil için React Native/Flutter/Swift/Kotlin olabilir.
- **Sorumluluklar:**
    - Kullanıcı arayüzünü sunar.
    - Kullanıcı etkileşimlerini alır ve bunları `FinTrack Web API`'sine RESTful API çağrılarına dönüştürür.
    - API'den gelen JWT'yi güvenli bir şekilde saklar ve sonraki isteklerde kullanır.

## 4. Teknoloji Stack'i
- **Backend:** .NET 8, Python 3.10, ASP.NET Core, FastAPI
- **Veritabanı:** PostgreSQL 15, Entity Framework Core
- **Konteynerleştirme:** Docker, Docker Compose
- **Mimari Stili:** Mikroservis-odaklı, RESTful API
- **Yetkilendirme:** JSON Web Tokens (JWT)

## 5. Detaylı Süreç Akışları (Sequence Diagrams)

Sistemin en kritik kullanıcı akışları aşağıda detaylandırılmıştır.

<details>
<summary><b>Akış 1: Yeni Kullanıcı Kaydı ve OTP ile Doğrulama</b> (Genişletmek için tıklayın)</summary>

```mermaid
sequenceDiagram
    participant User as Kullanıcı
    participant ClientApp as İstemci Uygulama
    participant FinTrackAPI as FinTrack API
    participant EmailService as E-posta Servisi
    participant Database as Veritabanı

    User->>ClientApp: Kayıt formunu doldurur (E-posta, Şifre)
    ClientApp->>FinTrackAPI: POST /api/auth/register

    FinTrackAPI->>Database: E-posta mevcut mu?
    alt E-posta Zaten Mevcut
        Database-->>FinTrackAPI: Evet
        FinTrackAPI-->>ClientApp: 400 Bad Request
    else E-posta Yeni
        Database-->>FinTrackAPI: Hayır
        FinTrackAPI->>FinTrackAPI: OTP ve son kullanma tarihi oluştur
        FinTrackAPI->>Database: Kullanıcıyı kaydet (IsVerified=false, OTP)
        FinTrackAPI->>EmailService: OTP'yi e-posta ile gönder
        FinTrackAPI-->>ClientApp: 201 Created
        ClientApp-->>User: Doğrulama ekranını göster
    end

    User->>ClientApp: OTP'yi girer
    ClientApp->>FinTrackAPI: POST /api/auth/verify-otp

    FinTrackAPI->>Database: Kullanıcıyı ve OTP'yi getir
    alt OTP Doğru ve Geçerli
        FinTrackAPI->>Database: Kullanıcıyı güncelle (IsVerified=true)
        FinTrackAPI-->>ClientApp: 200 OK (Doğrulandı)
    else OTP Yanlış veya Süresi Dolmuş
        FinTrackAPI-->>ClientApp: 400 Bad Request (Hatalı kod)
    end
```
</details>

<details>
<summary><b>Akış 2: Kullanıcı Girişi (Login) ve JWT Alma</b> (Genişletmek için tıklayın)</summary>

```mermaid
sequenceDiagram
    participant User as Kullanıcı
    participant ClientApp as İstemci Uygulama
    participant FinTrackAPI as FinTrack API
    participant Database as Veritabanı

    User->>ClientApp: Giriş bilgilerini girer (E-posta, Şifre)
    ClientApp->>FinTrackAPI: POST /api/auth/login

    FinTrackAPI->>Database: E-posta ile kullanıcıyı bul
    alt Kullanıcı Bulunamadı
        Database-->>FinTrackAPI: null
        FinTrackAPI-->>ClientApp: 401 Unauthorized
    else Kullanıcı Bulundu
        Database-->>FinTrackAPI: Kullanıcı verisi (Hash'li Şifre, IsVerified)
        FinTrackAPI->>FinTrackAPI: Şifre hash'ini doğrula
        alt Şifre Yanlış
            FinTrackAPI-->>ClientApp: 401 Unauthorized
        else Şifre Doğru
            alt Hesap Doğrulanmamış (IsVerified == false)
                FinTrackAPI-->>ClientApp: 403 Forbidden
            else Hesap Doğrulanmış
                FinTrackAPI->>FinTrackAPI: JWT (Access Token) oluştur
                FinTrackAPI-->>ClientApp: 200 OK (JWT ile birlikte)
                ClientApp->>ClientApp: Token'ı güvenli sakla
            end
        end
    end
```
</details>

## 6. Ağ ve Servis İletişimi (Networking)

- Tüm backend servisleri, `docker-compose.yml` içinde tanımlanan `fintrac_network` adlı özel bir "bridge" ağı üzerinde çalışır.
- Bu ağ sayesinde servisler, birbirleriyle doğrudan konteyner isimlerini kullanarak güvenli bir şekilde iletişim kurabilir. Örneğin, `FinTrack Web API` veritabanına `Host=db_postgres` adresi üzerinden erişir.
- Dış dünyaya sadece `fintrackwebapi` (Port `5000`) ve `finbotwebapi` (Port `5001`) servislerinin portları açılmıştır. Veritabanı portu (`5433`) ise sadece yerel makineden erişim ve yönetim kolaylığı için dışarıya açılmıştır, canlı (production) ortamlarda bu portun kapatılması önerilir.
```