# FinTrack – Sistem Rolleri ve Yetki Matrisi

Bu doküman, FinTrack sisteminde tanımlanmış kullanıcı ve operasyon rollerini, bu rollerin sorumluluklarını ve API endpoint'leri üzerindeki erişim yetkilerini detaylandırmaktadır. Sistem, "En Az Ayrıcalık Prensibi" (Principle of Least Privilege) temel alınarak tasarlanmıştır; her rol, görevini yerine getirmek için yalnızca gerekli olan minimum yetkiye sahiptir.

## 1. Uygulama Kullanıcı Rolleri

Bu roller, son kullanıcıların uygulama içinde sahip olduğu temel rollerdir.

### 1.1. Admin (Sistem Yöneticisi)

*   **Rolün Tanımı ve Amacı:** Sistemin tam kontrolüne sahip olan "süper kullanıcı". Uygulamanın genel sağlığından, kullanıcı yönetiminden, aboneliklerden ve kritik operasyonlardan sorumludur.
*   **Temel Sorumlulukları:**
    -   Tüm kullanıcıları listeleme, hesap durumlarını yönetme.
    -   Sistem geneli için istatistiksel ve finansal raporları görüntüleme.
    -   Abonelik planlarını yönetme ve kullanıcı aboneliklerini denetleme.
    -   Destek taleplerini yönetme ve üst düzey sorunlara müdahale etme.
    -   Sisteme "Çalışan" rollerini atama ve yönetme.
*   **API Yetkileri ve Kısıtlamaları:**
    -   `GET /api/users`: Tüm kullanıcıları listeleyebilir.
    -   `PUT /api/users/{id}/status`: Kullanıcı hesaplarını askıya alabilir veya aktive edebilir.
    -   `GET /api/reports/system-wide`: Sistem genelindeki toplu raporlara erişebilir.
    -   `GET /api/subscriptions`: Tüm abonelik kayıtlarını görebilir.
    -   `GET /api/transactions/{userId}`: **Kısıtlama:** Varsayılan olarak erişimi yoktur. Sadece bir destek durumu veya adli inceleme gibi özel durumlarda, tüm eylemleri loglanmak kaydıyla yetkilendirilmiş bir endpoint üzerinden erişebilir.
    -   `GET /api/debts/{userId}`: **Kısıtlama:** Kullanıcıların özel borç bilgilerine doğrudan erişemez. Anlaşmazlık çözümü için özel olarak tasarlanmış arayüzleri kullanır.

### 1.2. User (Standart Kullanıcı)

*   **Rolün Tanımı ve Amacı:** Sistemin ana kullanıcısı. Bireysel veya KOBİ sahibi olabilir. Sadece kendi finansal verilerini yönetir.
*   **Temel Sorumlulukları:**
    -   Kendi gelir, gider ve bütçelerini oluşturma ve yönetme.
    -   Borç ilişkileri oluşturma, alma ve video ile onaylama.
    -   Kendi finansal raporlarını ve analizlerini görüntüleme.
    -   (Eğer KOBİ ise) Kendi şirketine bağlı "Çalışan"ları yönetme.
*   **API Yetkileri ve Kısıtlamaları:**
    -   `GET, POST, PUT, DELETE /api/transactions`: **Kısıtlama:** Sadece `UserID`'si kendi token'ındaki `UserID` ile eşleşen verilere erişebilir. Başka bir kullanıcının verisine müdahale edemez.
    -   `GET, POST, PUT, DELETE /api/budgets`: Sadece kendi bütçelerini yönetebilir.
    -   `GET, POST, PUT /api/debts`: Sadece tarafı olduğu borç ilişkilerini yönetebilir.
    -   `POST /api/company/employees`: **Kısıtlama:** Sadece "Pro" veya "Enterprise" gibi uygun bir abonelik planına sahipse bu yetkiyi kullanabilir.
    -   `GET /api/users`: **Kısıtlama:** Bu endpoint'e erişimi **yoktur**.

### 1.3. Employee (Çalışan)

*   **Rolün Tanımı ve Amacı:** Bir "User" (KOBİ) tarafından sisteme eklenmiş, sınırlı yetkilere sahip kullanıcı. Genellikle masraf girişi gibi görevleri yerine getirir.
*   **Temel Sorumlulukları:**
    -   Bağlı olduğu şirket adına masraf/gider girişi yapmak.
    -   Kendi profil bilgilerini yönetmek.
*   **API Yetkileri ve Kısıtlamaları:**
    -   `POST /api/transactions`: **Kısıtlama:** Sadece "gider" türünde ve bağlı olduğu şirketin hesabına işlem ekleyebilir.
    -   `GET /api/transactions`: **Kısıtlama:** Sadece kendi eklediği işlemleri görebilir. Şirketin genel finansal durumuna erişemez.
    -   `GET, POST, PUT, DELETE /api/budgets`: Bu endpoint'lere erişimi **yoktur**.
    -   `GET, POST, PUT, DELETE /api/debts`: Bu endpoint'lere erişimi **yoktur**.

---

## 2. Sistem Operasyon Rolleri (Admin Paneli)

Bu roller, FinTrack ekibinin iç operasyonları için tanımlanmıştır ve Admin Paneli üzerinden işlem yaparlar.

### 2.1. Video Onay Operatörü (Verification Operator)

*   **Rolün Tanımı ve Amacı:** Borç sisteminin güvenliğini, kullanıcılar tarafından yüklenen onay videolarını inceleyerek sağlamak.
*   **API Yetkileri ve Kısıtlamaları:**
    -   `GET /api/admin/debt-verifications/pending`: Onay bekleyen borç işlemlerini listeler.
    -   `GET /api/admin/debt-verifications/{id}`: Sadece ilgili borcun video ve temel (anonimleştirilmiş) verilerini görüntüler.
    -   `POST /api/admin/debt-verifications/{id}/approve`: İncelenen videoyu onaylar.
    -   `POST /api/admin/debt-verifications/{id}/reject`: İncelenen videoyu reddeder.
    -   **Kısıtlama:** Sistemdeki başka hiçbir finansal veya kişisel veriye erişimi yoktur.

### 2.2. Teknik Destek Uzmanı (Technical Support Specialist)

*   **Rolün Tanımı ve Amacı:** Kullanıcıların yaşadığı teknik sorunları ve hesapla ilgili soruları çözmek.
*   **API Yetkileri ve Kısıtlamaları:**
    -   `GET /api/admin/support-tickets`: Destek taleplerini yönetir.
    -   `GET /api/admin/users/search?query={...}`: Kullanıcıları temel bilgilere göre arar.
    -   `GET /api/admin/users/{id}/profile`: **Kısıtlama:** Kullanıcının sadece finansal olmayan verilerini (isim, e-posta, abonelik türü, hesap durumu) görebilir.
    -   **Kısıtlama:** Kullanıcıların işlem listesi, bütçeleri, borçları gibi özel finansal verilerine kesinlikle erişemez. Tüm eylemleri denetim günlüğüne (audit log) kaydedilir.

### 2.3. Veri Analisti (Data Analyst)

*   **Rolün Tanımı ve Amacı:** İş kararlarına yön vermek için sistemdeki verilerden anonim ve toplu halde içgörüler üretmek.
*   **API Yetkileri ve Kısıtlamaları:**
    -   `GET /api/reports/subscriptions/summary`: Abonelik performansını gösteren toplu verileri alır.
    -   `GET /api/reports/transactions/anonymous-summary`: **Kısıtlama:** Tamamen anonimleştirilmiş ve toplulaştırılmış işlem verilerini (örn. kategoriye göre harcama dağılımı) alır.
    -   **Kısıtlama:** Hiçbir koşulda bireysel kullanıcı verilerine veya kişisel bilgilere erişimi yoktur. Eriştiği tüm endpoint'ler, backend tarafından önceden işlenmiş anonim veriler sunar.

## 3. Yetki Matrisi Özeti

| Kontrolür | Endpoint | Admin | User | Employee | Video Op. | Tech Support | Data Analyst |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Auth Controller** | `POST /api/auth/user/initiate-registration` | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Auth Controller** | `POST /api/auth/users/verify-otp-and-register` | ✅ | ❌ | ❌ | ❌ | ⚠️¹ | ❌ |
| **Auth Controller** | `POST /api/auth/login` | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Budget Controller** | `GET /api/budgets` | ⚠️² | ✅ | ⚠️³ | ❌ | ❌ | ❌ |
| **Budget Controller** | `GET /api/budgets/{id}` | ⚠️² | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Budget Controller** | `POST /api/budgets` | ✅ | ❌ | ❌ | ✅ | ❌ | ❌ |
| **Budget Controller** | `PUT /api/budgets/{id}` | ✅ | ❌ | ❌ | ❌ | ✅ | ❌ |
| **Budget Controller** | `DELETE /api/budgets/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Budget Controller** | `GET /api/budgets/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Account Controller** | `GET /api/account` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Account Controller** | `GET /api/account/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Account Controller** | `POST /api/account` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Account Controller** | `PUT /api/account/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Account Controller** | `DELETE /api/account/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Chat Controller** | `POST /api/chat/send` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Categories Controller** | `GET /api/categories` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Categories Controller** | `GET /api/categories/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Categories Controller** | `POST /api/categories` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Categories Controller** | `PUT /api/categories/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Categories Controller** | `DELETE /api/categories/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **BudgetCategory Controller** | `GET /api/budgetcategory` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **BudgetCategory Controller** | `GET /api/budgetcategory/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **BudgetCategory Controller** | `POST /api/budgetcategory` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **BudgetCategory Controller** | `PUT /api/budgetcategory/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **BudgetCategory Controller** | `DELETE /api/budgetcategory/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Membership Controller** | `GET /api/membership/current` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Membership Controller** | `GET /api/membership/history` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Membership Controller** | `POST /api/membership/create-checkout-session` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Membership Controller** | `POST /api/membership/{id}/cansel` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Transactions Controller** | `GET /api/transactions` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Transactions Controller** | `GET /api/transactions/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Transactions Controller** | `GET /api/transactions/category-type/{type}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Transactions Controller** | `GET /api/transactions/category-name/{name}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Transactions Controller** | `PUT /api/transactions/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Transactions Controller** | `POST /api/transactions` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Transactions Controller** | `DELETE /api/transactions/{id}` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **UserSettings Controller** | `GET /api/usersettings` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **UserSettings Controller** | `POST /api/usersettings` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **UserSettings Controller** | `PUT /api/usersettings` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **UserSettings Controller** | `DELETE /api/usersettings` | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |

**Açıklamalar:**
-   `✅`: Tam Yetkili
-   `❌`: Yetkisi Yok
-   `⚠️¹`: Sadece finansal olmayan temel profil verilerini görebilir.
-   `⚠️²`: Sadece özel durumlarda ve tüm eylemleri loglanarak erişim sağlanır.
-   `⚠️³`: Sadece kendi eklediği işlemleri görebilir.