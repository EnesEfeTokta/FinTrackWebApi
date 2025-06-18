# Changelog

Bu projedeki tüm önemli değişiklikler bu dosyada belgelenecektir.

Bu dosyanın formatı [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) standardına dayanmaktadır ve bu proje [Semantic Versioning](https://semver.org/spec/v2.0.0.html) (Anlamsal Sürümleme) prensiplerine uyar.

## [Unreleased] - Henüz Yayınlanmadı

_Bu bölüm, bir sonraki sürümde yer alacak ancak henüz yayınlanmamış değişiklikleri içerir._

### Added (Eklendi)
- İşlemlere (`Transaction`) not ekleme özelliği.
- Kullanıcı profili için `/api/profile` endpoint'i.

### Changed (Değiştirildi)
- `AccountController` içerisindeki loglama (kayıt tutma) yapısı daha detaylı hata takibi için iyileştirildi.
- Token geçerlilik süresi güvenlik nedeniyle 1 saat olarak güncellendi.

### Fixed (Düzeltildi)
- Bir işlem silindiğinde hesap bakiyesinin yanlış hesaplanmasına neden olan hata giderildi.
- `DELETE /api/categories/{id}` endpoint'inin, ilişkili işlemleri olan bir kategoriyi silmeye çalışırken `500 Internal Server Error` vermesi sorunu düzeltildi. Artık bu durumda `400 Bad Request` ile bilgilendirici bir mesaj dönüyor.


---

## [1.0.0] - 2023-11-15

### Added (Eklendi)
- **Projenin İlk Kararlı Sürümü!**
- Docker ve Docker Compose ile tam geliştirme ortamı desteği.
- `FinTrackWebApi` (.NET 8) için temel CRUD (Oluştur, Oku, Güncelle, Sil) operasyonları:
  - Kullanıcı Yetkilendirme (`/api/auth`) - Login ve Register.
  - Hesap Yönetimi (`/api/accounts`).
  - Kategori Yönetimi (`/api/categories`).
  - Bütçe Yönetimi (`/api/budgets`).
  - İşlem Yönetimi (`/api/transactions`).
- `FinBotWebApi` (Python) için ilk yapı ve entegrasyon.
- PostgreSQL veritabanı entegrasyonu.
- Proje için temel dokümantasyonlar oluşturuldu: `README.md`, `FAQ.md`, `CONTRIBUTING.md`, ve bu `CHANGELOG.md` dosyası.
- Swagger UI (`/swagger`) ile interaktif API dokümantasyonu.

---

## [0.2.0] - 2023-10-20

### Added (Eklendi)
- JWT (JSON Web Token) tabanlı yetkilendirme sistemi eklendi. Tüm hassas endpoint'ler koruma altına alındı.
- Bütçeler (`Budgets`) ve İşlemler (`Transactions`) için controller'lar ve iş mantığı eklendi.

### Changed (Değiştirildi)
- Proje .NET 7'den .NET 8'e yükseltildi.

### Fixed (Düzeltildi)
- Kategori isimlerinin büyük/küçük harfe duyarlı olması ve aynı ismin farklı formatlarda kaydedilebilmesi sorunu giderildi.

---

## [0.1.0] - 2023-09-30

### Added (Eklendi)
- Projenin ilk başlangıcı.
- `FinTrackWebApi` projesinin temel yapısı oluşturuldu.
- `Account` ve `Category` modelleri için yetkilendirme olmadan çalışan temel CRUD endpoint'leri eklendi.
- Veritabanı olarak PostgreSQL yapılandırıldı ve Entity Framework Core ile ilk migration'lar oluşturuldu.