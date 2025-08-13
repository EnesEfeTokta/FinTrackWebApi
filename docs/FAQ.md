# FinTrack Projesi - Sıkça Sorulan Sorular (SSS)

Bu doküman, FinTrack projesi, mimarisi, API kullanımı ve geliştirme ortamıyla ilgili sıkça sorulan sorulara hızlı ve net yanıtlar sunmak amacıyla hazırlanmıştır.

## İçindekiler
*   [Genel ve Başlarken](#genel-ve-başlarken)
*   [API Mimarisi ve Kullanımı](#api-mimarisi-ve-kullanımı)
*   [Yetkilendirme (Authentication & Authorization)](#yetkilendirme-authentication--authorization)
*   [Geliştirme Ortamı ve DevOps](#geliştirme-ortamı-ve-devops)
*   [Veritabanı](#veritabanı)

---

## Genel ve Başlarken

### **Soru:** Bu proje nedir ve temel amacı nedir?
**Cevap:** FinTrack, bireysel ve profesyonel kullanıcıların finansal hayatlarını yönetmelerini sağlayan bir SaaS (Hizmet Olarak Yazılım) platformudur. Bu repository, platformun **backend (sunucu tarafı)** hizmetlerini içerir ve istemci uygulamaları (WPF, mobil vb.) için güvenli ve zengin bir API altyapısı sunar.

### **Soru:** Projenin ana dokümantasyonlarına nereden ulaşabilirim?
**Cevap:** Projenin tüm teknik detayları `docs/` klasörü altında merkezi bir yapıda toplanmıştır:
1.  **`README.md`:** Projeye genel bir bakış, teknoloji yığını ve hızlı başlangıç bilgileri içerir.
2.  **`docs/ARCHITECTURE.md`:** Sistemin üst düzey mimarisini, servislerini ve veri akışlarını detaylandırır.
3.  **`docs/DATABASE.md`:** Veritabanı şemasını, tabloları, ilişkileri ve ERD'yi içerir.
4.  **`docs/api/` Klasörü:** Her bir API controller'ı için hazırlanmış detaylı endpoint dokümanlarını barındırır.
5.  **Swagger (Etkileşimli UI):** Proje çalışırken `http://localhost:5246/swagger` adresinden ulaşılabilen, endpoint'leri canlı olarak test etmenizi sağlayan arayüzdür.

### **Soru:** Projenin güncel durumu nedir?
**Cevap:** Proje aktif olarak geliştirme aşamasındadır. `README.md` dosyası ana branch'in son derleme durumunu gösterir.

---

## API Mimarisi ve Kullanımı

### **Soru:** API'nin ana (base) URL adresi nedir?
**Cevap:** Docker ile yerel geliştirme ortamında çalışan servislerin varsayılan adresi: `http://localhost:5246`'dir.

### **Soru:** API'de tarih ve saat formatı olarak ne kullanılmalı?
**Cevap:** API genelinde **ISO 8601** standardı (`YYYY-MM-DDTHH:mm:ssZ`) kullanılmaktadır. Tüm tarih ve saat verileri sunucuya **UTC** (Coordinated Universal Time) olarak gönderilmeli ve sunucudan bu formatta alınmalıdır.

### **Soru:** `Status`, `Type` gibi alanlar neden sayı yerine metin (`"Active"`, `"Income"`) olarak gönderiliyor?
**Cevap:** Bu, API'nin okunabilirliğini ve geliştirici dostu olmasını artırmak için bilinçli bir tasarım tercihidir. ASP.NET Core, bu metin değerlerini sunucu tarafında otomatik olarak doğru `enum` tiplerine dönüştürür. Bu sayede, API'yi kullanan birinin sayıların ne anlama geldiğini ezberlemesine gerek kalmaz.

### **Soru:** Bir `DELETE` işlemi neden `204 No Content` döndürüyor?
**Cevap:** Bu bir hata değildir. `204 No Content` HTTP durum kodu, işlemin (örn: silme) başarıyla tamamlandığını ancak sunucunun yanıt gövdesinde geri döndüreceği bir içerik olmadığını belirtir. Bu, RESTful API tasarımında yaygın ve doğru bir yaklaşımdır.

---

## Yetkilendirme (Authentication & Authorization)

### **Soru:** API'ye erişim için nasıl kimlik doğrularım?
**Cevap:** İki aşamalı bir süreçle:
1.  **Kayıt:** `POST /UserAuth/initiate-registration` ve `POST /UserAuth/verify-otp-and-register` endpoint'leri ile OTP doğrulamalı bir kayıt işlemi yapmanız gerekir.
2.  **Giriş:** Kayıt sonrası `POST /UserAuth/login` endpoint'ine e-posta ve şifrenizi göndererek bir **JWT (JSON Web Token)** alabilirsiniz.

### **Soru:** Aldığım JWT'yi nasıl kullanmalıyım?
**Cevap:** Aldığınız `accessToken`'ı, yetkilendirme gerektiren tüm isteklerin `Authorization` HTTP başlığına `Bearer ` ön eki ile eklemelisiniz. **Örnek:** `Authorization: Bearer eyJhbGciOiJIUzI1Ni...`

### **Soru:** `401 Unauthorized` ile `403 Forbidden` arasındaki fark nedir?
**Cevap:**
*   **401 Unauthorized:** Kimliğiniz doğrulanamadı demektir. Sisteme "giriş yapmamış" olarak kabul edilirsiniz. Genellikle token'ın eksik, geçersiz veya süresinin dolmuş olması durumunda alınır.
*   **403 Forbidden:** Kimliğiniz doğrulandı, yani "giriş yapmışsınız", ancak erişmeye çalıştığınız kaynak veya eylem için yetkiniz yok. Örneğin, `User` rolündeki bir kullanıcının sadece `Admin` rolünün erişebileceği bir endpoint'e istek atması bu hatayı döndürür.

---

## Geliştirme Ortamı ve DevOps

### **Soru:** Projeyi yerel bilgisayarımda nasıl çalıştırabilirim?
**Cevap:** Projeyi çalıştırmanın **tek ve önerilen yolu Docker'dır**. Projenin ana dizininde `docker-compose up --build` komutunu çalıştırmanız yeterlidir. Bu komut, tüm servisleri (API'ler, veritabanları, gözlem araçları) doğru yapılandırmalarla birlikte otomatik olarak başlatacaktır.

### **Soru:** Neden Docker kullanmak zorunlu?
**Cevap:** Docker, projenin tüm bağımlılıklarını (belirli .NET ve Python versiyonları, PostgreSQL, Ollama vb.) izole konteynerler içine paketler. Bu sayede, "benim makinemde çalışıyordu" sorununu tamamen ortadan kaldırır ve her geliştiricinin aynı ortamda çalışmasını garanti eder.

### **Soru:** Belirli bir servisin loglarını nasıl görebilirim?
**Cevap:** Yeni bir terminal açın ve `docker-compose logs -f <servis_adi>` komutunu kullanın. **Örnek:** `docker-compose logs -f fintrack_api`

### **Soru:** Gözlem (monitoring) panosuna (Grafana) nasıl erişebilirim?
**Cevap:** Proje çalışırken tarayıcınızdan `http://localhost:3000` adresine gidin. Grafana, sistemin genel sağlık durumunu, CPU/RAM kullanımını ve API performansını gösteren panoları sunar.

### **Soru:** Hassas bilgileri (API anahtarları, şifreler) nerede saklamalıyım?
**Cevap:** **Asla doğrudan `appsettings.json` içine yazmayın!** Yerel geliştirme için, ASP.NET Core'un "User Secrets" özelliğini kullanın. Üretim ortamları için ise bu bilgileri **ortam değişkenleri (environment variables)** veya Azure Key Vault gibi güvenli bir konfigürasyon yönetim aracı üzerinden sağlayın.

---

## Veritabanı

### **Soru:** Geliştirme veritabanına bir istemci (DBeaver, pgAdmin) ile nasıl bağlanabilirim?
**Cevap:** `docker-compose.yml` dosyasında veritabanı portu (`5432`) makinenizin `5432` portuna yönlendirilmiştir. Aşağıdaki bilgilerle bağlantı kurabilirsiniz:
*   **Host:** `localhost`
*   **Port:** `5432`
*   **Veritabanı (MainDB):** `fintrack_main_db`
*   **Kullanıcı Adı:** `postgres`
*   **Şifre:** `your_strong_password` (docker-compose.yml dosyasından kontrol edin)

### **Soru:** `MainDB` ve `LogDB` arasındaki fark nedir?
**Cevap:**
*   **`MainDB`:** Ana uygulama verilerinin (kullanıcılar, hesaplar vb.) tutulduğu birincil veritabanıdır.
*   **`LogDB`:** `MainDB` üzerinde gerçekleşen her veri değişikliğini (Ekleme, Güncelleme, Silme) denetim amacıyla kaydeden ikincil veritabanıdır. Bu, tam bir izlenebilirlik sağlar.

### **Soru:** `docker-compose down` komutunu çalıştırdığımda veritabanı verilerim silinir mi?
**Cevap:** **Hayır, silinmez.** Veritabanı verileriniz, `postgres_data` ve `postgres_log_data` adlı Docker "volume"lerinde saklanır. Bu volume'ler, konteynerler durdurulup kaldırılsa bile verilerinizi kalıcı olarak korur. Verileri tamamen sıfırlamak isterseniz `docker-compose down -v` komutunu kullanmanız gerekir.