# FinTrack Projesi - Sıkça Sorulan Sorular (SSS)

Bu doküman, FinTrack projesiyle ilgili sıkça sorulan sorulara yanıtlar içermektedir.

## İçindekiler

*   [Genel ve Başlarken](#genel-ve-başlarken)
*   [Yetkilendirme (Authentication)](#yetkilendirme-authentication)
*   [API Kullanımı](#api-kullanımı)
*   [Geliştirme Ortamı ve Katkı](#geliştirme-ortamı-ve-katkı)
*   [Destek ve İletişim](#destek-ve-iletişim)

---

## Genel ve Başlarken

### **Soru:** Bu proje nedir ve ne işe yarar?
**Cevap:** FinTrack, kullanıcıların kişisel finanslarını (hesaplar, işlemler, bütçeler vb.) yönetmelerine olanak tanıyan bir web API hizmetidir. Frontend uygulamaları (web, mobil) için bir backend altyapısı sunar.

### **Soru:** API dokümantasyonuna nereden ulaşabilirim?
**Cevap:** Projenin tüm endpoint'lerini, modellerini ve kullanım örneklerini içeren iki ana dokümantasyon kaynağımız var:
1.  **Markdown Dokümanları:** Projenin `Documents/` klasöründe bulunan statik dokümanlar.
2.  **Swagger (Etkileşimli UI):** Proje çalışırken `https://api.ornekdomain.com/swagger` adresinden ulaşılabilen, endpoint'leri canlı olarak test etmenize olanak tanıyan arayüz.

### **Soru:** API'nin ana (base) URL adresleri nelerdir?
**Cevap:**
*   **Geliştirme (Development):** `https://localhost:7001`
*   **Test (Staging):** `https://staging-api.fintrack.com` *(Geçerli Değil Hala)*
*   **Canlı (Production):** `https://api.fintrack.com`*(Geçerli Değil Hala)*

---

## Yetkilendirme (Authentication)

### **Soru:** API'ye erişim için nasıl token (jeton) alabilirim?
**Cevap:** `POST /api/auth/login` endpoint'ine kullanıcı adı ve şifrenizi göndererek bir Bearer Token alabilirsiniz. Bu endpoint herkese açıktır.

### **Soru:** Aldığım token'ı nasıl kullanmalıyım?
**Cevap:** Aldığınız token'ı, yetkilendirme gerektiren tüm isteklerin `Authorization` başlığına (header) `Bearer` ön eki ile eklemelisiniz.

### **Soru:** Neden `401 Unauthorized` hatası alıyorum?
**Cevap:** Bu hata genellikle şu nedenlerden kaynaklanır:
1.  İsteğinizde `Authorization` başlığı bulunmuyor.
2.  Token süresi dolmuş (genellikle 1 saat veya 24 saat sonra).
3.  Gönderdiğiniz token geçersiz veya hatalı.

### **Soru:** `401 Unauthorized` ile `403 Forbidden` arasındaki fark nedir?
**Cevap:**
*   **401 Unauthorized:** Kimliğiniz doğrulanmamış demektir. Yani sisteme "giriş yapmamışsınız" veya token'ınız geçersiz.
*   **403 Forbidden:** Kimliğiniz doğrulanmış ancak erişmeye çalıştığınız kaynak için yetkiniz yok. Örneğin, normal bir `User` rolündeki kullanıcı, sadece `Admin` rolünün erişebileceği bir endpoint'e istek atarsa bu hatayı alır.

---

## API Kullanımı

### **Soru:** Tarih ve saat formatı ne olmalı?
**Cevap:** API genelinde tarih/saat formatı olarak **ISO 8601** standardı (`YYYY-MM-DDTHH:mm:ssZ`) kullanılmaktadır. Tüm tarih ve saat verileri sunucuya **UTC** (Coordinated Universal Time) olarak gönderilmeli ve sunucudan bu formatta alınmalıdır.

### **Soru:** Bir endpoint neden `204 No Content` döndürüyor? Bu bir hata mı?
**Cevap:** Hayır, bu bir hata değildir. `204 No Content` status kodu, işlemin başarıyla tamamlandığını ancak sunucunun geri döndüreceği bir içerik olmadığını belirtir. Genellikle başarılı bir `PUT` (güncelleme) veya `DELETE` (silme) işleminden sonra bu yanıt alınır.

### **Soru:** API'de sayfalama (pagination) nasıl çalışıyor?
**Cevap:** Liste döndüren (örneğin `GET /api/transactions`) endpoint'lerde sayfalama için şu query parametrelerini kullanabilirsiniz:
*   `pageNumber`: İstenen sayfa numarası (varsayılan: `1`).
*   `pageSize`: Her sayfadaki kayıt sayısı (varsayılan: `10`, maksimum: `50`).

**Örnek:** `GET /api/transactions?pageNumber=2&pageSize=20` (2. sayfadaki 20 işlemi getirir).

---

## Geliştirme Ortamı ve Katkı

### **Soru:** Projenin teknik altyapısı nedir?
**Cevap:** Proje, `docker-compose` ile yönetilen üç ana servisten oluşur:
1.  **fintrackwebapi:** Ana iş mantığını içeren .NET 8 API'si.
2.  **finbotwebapi:** Yardımcı görevler için kullanılan Python 3.10 (FastAPI/Uvicorn) API'si.
3.  **db_postgres:** Veri depolama için kullanılan PostgreSQL 15 veritabanı.

Bu servisler, `fintrac_network` adında ortak bir Docker ağı üzerinden haberleşir.

### **Soru:** Projeyi yerel bilgisayarımda nasıl çalıştırabilirim?
**Cevap:** Projeyi çalıştırmanın **tek ve önerilen yolu Docker'dır**. Aşağıdaki adımları izleyin:

**Gereksinimler:**
*   [Docker](https://www.docker.com/products/docker-desktop/)
*   [Docker Compose](https://docs.docker.com/compose/install/) (Genellikle Docker Desktop ile birlikte gelir)

**Kurulum Adımları:**
1.  Proje reposunu bilgisayarınıza klonlayın: `git clone <repo_url>`
2.  `FinBotWebApi` servisi için `.env` dosyasını oluşturun. Projenin ana dizinindeyken `FinBotWebApi` klasörünün içine gidin ve `.env` adında bir dosya oluşturup gerekli ortam değişkenlerini ekleyin.
3.  Projenin ana dizinine geri dönün ve tüm servisleri başlatmak için terminalde şu komutu çalıştırın:
    ```bash
    docker-compose up --build
    ```
    *   Eğer servisleri arkaplanda çalıştırmak isterseniz `-d` parametresini ekleyebilirsiniz: `docker-compose up -d --build`
4.  Servislerin loglarını (kayıtlarını) görmek için:
    ```bash
    docker-compose logs -f fintrack_api
    ```
5.  Tüm servisleri durdurmak ve container'ları kaldırmak için:
    ```bash
    docker-compose down
    ```

### **Soru:** Geliştirme veritabanına bir istemci (DBeaver, DataGrip vb.) ile nasıl bağlanabilirim?
**Cevap:** `docker-compose.yml` dosyasında veritabanı portu (`5432`) makinenizin `5433` portuna yönlendirilmiştir. Aşağıdaki bilgilerle bağlantı kurabilirsiniz:
*   **Host:** `localhost`
*   **Port:** `5433`
*   **Veritabanı Adı:** `myfintrackdb`
*   **Kullanıcı Adı:** `postgres`
*   **Şifre:** `140xxx-+`

**Not:** Veritabanı verileriniz, `postgres_data` adlı bir Docker volume'ünde saklandığı için `docker-compose down` yapsanız bile verileriniz silinmez.

### **Soru:** Projede kullanılan teknolojilerin versiyonları nelerdir?
**Cevap:**
*   **Ana API:** .NET 8
*   **Yardımcı API:** Python 3.10
*   **Veritabanı:** PostgreSQL 15

---

## Destek ve İletişim

### **Soru:** Sorum bu listede yok. Ne yapmalıyım?
**Cevap:** Lütfen aşağıdaki adımları izleyin:
1.  Öncelikle projenin diğer dokümanlarını (`README.md`, `Documents/` klasörü) kontrol edin.
2.  Projenin [Issue Tracker Linki]'nde daha önce benzer bir sorunun sorulup sorulmadığını araştırın.
3.  Eğer yanıt bulamazsanız, yeni bir "issue" (sorun/talep) oluşturarak sorunuzu detaylı bir şekilde açıklayın.
4.  Acil durumlar için [İlgili Kişi/Kanal] ile iletişime geçebilirsiniz.