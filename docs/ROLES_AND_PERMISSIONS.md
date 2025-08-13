# FinTrack – Sistem Rolleri ve Yetki Matrisi

Bu doküman, FinTrack sisteminde tanımlanmış rolleri, bu rollerin sorumluluklarını ve API endpoint'leri üzerindeki erişim yetkilerini detaylandırmaktadır. Sistem, **"En Az Ayrıcalık Prensibi" (Principle of Least Privilege)** temel alınarak tasarlanmıştır; her rol, görevini yerine getirmek için yalnızca gerekli olan minimum yetkiye sahiptir.

## 1. Sistemde Tanımlı Roller

### 1.1. User (Standart Kullanıcı)

*   **Rolün Tanımı ve Amacı:** Sistemin ana son kullanıcısıdır. FinTrack'in tüm temel özelliklerini kendi kişisel verileri üzerinde kullanmak için sisteme kaydolan herkestir.
*   **Temel Sorumlulukları:**
    *   Kendi finansal hesaplarını, bütçelerini, gelir/gider işlemlerini ve kategorilerini oluşturmak, görüntülemek ve yönetmek.
    *   Güvenli Borç Sistemi'nde "Alacaklı" veya "Borçlu" olarak yer almak, teklif oluşturmak, yanıtlamak ve video yüklemek.
    *   Kendi verilerinden raporlar oluşturmak.
    *   Uygulama ve bildirim ayarlarını kişiselleştirmek.
    *   Sistemle ilgili geri bildirimde bulunmak.
*   **Temel Kısıtlama:** Bu roldeki bir kullanıcı, **hiçbir koşulda başka bir kullanıcının finansal veya kişisel verisine erişemez.** Tüm API çağrıları, token'dan gelen `UserId` ile filtrelenir.

### 1.2. Admin (Sistem Yöneticisi)

*   **Rolün Tanımı ve Amacı:** Sistemin genel yönetiminden ve operasyonel bütünlüğünden sorumlu olan "süper kullanıcıdır". Bu rol, genellikle FinTrack'in iç ekibi tarafından kullanılır.
*   **Temel Sorumlulukları:**
    *   Kullanıcı hesaplarını yönetmek (askıya alma, silme vb. - *geliştirilecek*).
    *   Sistem genelindeki abonelik planlarını oluşturmak ve yönetmek.
    *   Güvenli Borç Sistemi'ndeki kritik adımları denetlemek (örn: video onayı).
    *   Sistem sağlığını izlemek ve loglara erişmek.
*   **Temel Kısıtlama:** `Admin` rolü geniş yetkilere sahip olsa da, kullanıcıların özel finansal verilerine (işlem detayları gibi) doğrudan erişimi, denetim kaydı (audit log) ve katı protokoller altında sınırlandırılmıştır.

### 1.3. VideoApproval (Video Onay Operatörü)

*   **Rolün Tanımı ve Amacı:** Bu rol, Güvenli Borç Sistemi'nin (GBS) güvenliğini sağlamakla görevli, son derece sınırlı yetkilere sahip bir operasyonel roldür.
*   **Temel Sorumlulukları:**
    *   Kullanıcılar tarafından borç taahhüdü için yüklenen videoları incelemek.
    *   İnceleme sonucuna göre videoyu onaylamak veya reddetmek.
*   **Temel Kısıtlama:** Bu rol, GBS onay süreci dışındaki **hiçbir sistemsel veya kişisel veriye erişemez.** Tek yetkisi, `VideosController` altındaki onaylama (`video-approve`) endpoint'ini çağırmaktır.

## 2. Yetki Matrisi

Aşağıdaki matris, projedeki her bir controller'ın endpoint'lerine hangi rollerin erişebileceğini göstermektedir. Bu matris, kod üzerindeki `[Authorize(Roles = "...")]` ve `[AllowAnonymous]` attribute'ları incelenerek oluşturulmuştur.

*   `✅`: Rolün endpoint'e erişim yetkisi var.
*   `❌`: Rolün endpoint'e erişim yetkisi yok.
*   `🔑`: Endpoint halka açık, yetkilendirme gerektirmez.

| Controller | Rol: User | Rol: Admin | Rol: VideoApproval | Notlar / Kısıtlamalar |
| :--- | :---: | :---: | :---: | :--- |
| **AccountController** | ✅ | ✅ | ❌ | Kullanıcı sadece kendi hesaplarını yönetebilir. |
| **BudgetsController** | ✅ | ✅ | ❌ | Kullanıcı sadece kendi bütçelerini yönetebilir. |
| **CategoriesController** | ✅ | ✅ | ❌ | Kullanıcı sadece kendi kategorilerini yönetebilir. |
| **ChatController** | ✅ | ✅ | ❌ | Sohbet oturumu kullanıcıya özeldir. |
| **DebtController** | ✅ | ✅ | ❌ | Kullanıcı sadece tarafı olduğu borçları yönetebilir. |
| **FeedbackController** | ✅ | ✅ | ❌ | Kullanıcı sadece kendi geri bildirimlerini yönetebilir. |
| **LogController** | 🔑 | 🔑 | 🔑 | **Halka Açık!** Güvenliği ağ katmanında sağlanmalıdır. |
| **MembershipController** | ✅ | ✅ | ❌ | `.../plan` endpoint'leri yöneticiye özeldir. `AllowAnonymous` plan listeleme hariç. |
| **NotificationController** | ✅ | ✅ | ❌ | Kullanıcı sadece kendi bildirimlerini yönetebilir. |
| **ReportsController** | ✅ | ✅ | ❌ | Kullanıcı sadece kendi verilerinden rapor oluşturabilir. |
| **StripeWebhookController** | 🔑 | 🔑 | 🔑 | **Halka Açık!** Stripe'tan gelen istekler için. Güvenliği imza ile sağlanır. |
| **TransactionCategoryController** | ✅ | ✅ | ❌ | Kullanıcı sadece kendi işlem kategorilerini yönetebilir. |
| **TransactionsController** | ✅ | ✅ | ❌ | Kullanıcı sadece kendi işlemlerini yönetebilir. |
| **UserController** | ✅ | ❌ | ❌ | Sadece `User` rolüne özeldir. Kullanıcı kendi profilini çeker. |
| **UserSettingsController** | ✅ | ✅ | ❌ | Kullanıcı sadece kendi ayarlarını yönetebilir. |
| **UserAuthController** | 🔑 | 🔑 | 🔑 | **Halka Açık!** Kayıt ve giriş işlemleri için. |
| **VideosController** | ✅ | ✅ | ✅ | `.../user-upload-video`: `User`'a özel. <br> `.../video-approve`: `Admin`/`VideoApproval`'a özel. <br> `.../video-metadata-stream`: Borcun alacaklısı olan `User`'a özel. |