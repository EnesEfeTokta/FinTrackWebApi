# FinTrack API: Stripe Webhook Alıcısı

Bu doküman, Stripe'tan gelen gerçek zamanlı olayları (event) dinleyen ve ödeme süreçlerini otomatikleştiren `StripeWebhookController` endpoint'ini açıklamaktadır.

*Controller Base Path:* `/api/stripe/webhook`

---

## Genel Bilgiler

### Yetkilendirme ve Güvenlik

*   **Endpoint:** Bu endpoint **halka açıktır** (`AllowAnonymous`), çünkü Stripe servisinin kimlik doğrulaması olmadan buraya istek gönderebilmesi gerekir.
*   **Webhook Güvenliği (Signature Verification):** Endpoint halka açık olsa da, her gelen isteğin gerçekten Stripe'tan geldiğini doğrulamak için bir güvenlik mekanizması kullanılır. Stripe, her isteğe `Stripe-Signature` adında özel bir HTTP başlığı ekler. Controller, bu imzayı `appsettings.json` dosyasında saklanan gizli bir anahtar (`WebhookSecret`) ile karşılaştırarak isteğin meşruluğunu doğrular. **İmza geçersizse, istek reddedilir.** Bu, sahte ödeme bildirimlerini engeller.

### Mimarideki Rolü: Olay Güdümlü Otomasyon

Bu controller, doğrudan kullanıcılar tarafından çağrılmaz. Bir **olay dinleyicisi (event listener)** olarak görev yapar.

1.  **Ödeme Başarılı:** Bir kullanıcı, `MembershipController` üzerinden başlattığı Stripe ödeme sayfasında ödemeyi başarıyla tamamlar.
2.  **Stripe Olay Gönderir:** Stripe, bu başarılı ödeme olayını (`checkout.session.completed`) önceden yapılandırılmış olan bu webhook endpoint'ine bir `POST` isteği ile bildirir.
3.  **Webhook İşlemi:** `StripeWebhookController`, bu isteği alır ve aşağıdaki işlemleri otomatik olarak gerçekleştirir:
    *   İsteğin imzasını doğrular.
    *   Olayın içindeki verileri (ödeme ID'si, üyelik ID'si vb.) ayrıştırır.
    *   İlgili kullanıcının veritabanındaki üyelik durumunu `PendingPayment`'tan `Active`'e günceller.
    *   Ödeme kaydını `Succeeded` olarak işaretler.
    *   Kullanıcıya başarılı ödeme ve fatura detaylarını içeren bir onay e-postası gönderir.

Bu yapı, ödeme ve üyelik aktivasyon sürecini insan müdahalesi olmadan, güvenli ve otomatik bir şekilde yönetir.

---

## Endpoints

### 1. Stripe Webhook Olaylarını İşle

Stripe tarafından gönderilen tüm webhook olaylarını kabul eden ve işleyen tekil endpoint.

*   **Endpoint:** `POST /api/stripe/webhook`
*   **Açıklama:** Bu endpoint, yalnızca `checkout.session.completed` olayını aktif olarak işler. Diğer olay türleri şu an için loglanır ancak bir eyleme neden olmaz.
*   **Yetkilendirme:** Gerekmez (`AllowAnonymous`). Güvenlik imza doğrulaması ile sağlanır.

#### Request Body

*   **Content-Type:** `application/json`
*   **İçerik:** Bu isteğin gövdesi doğrudan Stripe tarafından oluşturulur ve `Stripe.Event` nesne yapısına sahiptir. Manuel olarak oluşturulması gerekmez.

#### Başarılı Yanıt (Success Response)

*   **Status Code:** `200 OK`
*   **Açıklama:** Bu endpoint, Stripe'a "olayı başarıyla aldım ve işledim" mesajını vermek için **her zaman** `200 OK` döner (imza hatası veya kritik sunucu hatası hariç). Bu, Stripe'ın aynı olayı tekrar tekrar göndermesini engeller.
*   **Content:** Yanıt gövdesi boştur.

#### Hata Yanıtları (Error Responses)

*   `400 Bad Request`:
    *   `Stripe-Signature` başlığı geçersiz veya eksikse.
    *   Stripe olayının içindeki veriler (metadata) eksik veya hatalı ise.
*   `404 Not Found`:
    *   Stripe olayındaki metadata içinde belirtilen `UserMembershipId` veya `PaymentId` veritabanında bulunamazsa.
*   `500 Internal Server Error`:
    *   Veritabanı işlemleri veya e-posta gönderimi sırasında beklenmedik bir hata oluşursa.

Bu hatalar oluşsa bile, endpoint mümkün olduğunca Stripe'a `200 OK` dönmeye çalışır ve hatayı loglar. Kritik durumlarda (imza hatası gibi) `4xx` döner.