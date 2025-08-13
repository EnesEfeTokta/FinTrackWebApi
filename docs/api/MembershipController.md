# FinTrack API: Üyelik ve Plan Yönetimi (Membership Controller)

Bu doküman, FinTrack'in üyelik planlarını, kullanıcı üyeliklerini ve Stripe üzerinden ödeme süreçlerini yöneten `MembershipController` endpoint'lerini açıklamaktadır. Bu controller, hem son kullanıcıya yönelik hem de yönetici (Admin) fonksiyonlarını barındırır.

*Controller Base Path:* `/Membership`

---

## Endpoint Kategorileri

Bu controller'daki endpoint'ler üç ana gruba ayrılır:

1.  **Abonelik Planları (Plans):** Herkese açık (`AllowAnonymous`) endpoint'lerdir. Kullanıcıların ve potansiyel müşterilerin mevcut üyelik planlarını ve özelliklerini görmesini sağlar.
2.  **Yönetici Fonksiyonları (Admin-Only):** Sadece `Admin` rolüne sahip kullanıcıların üyelik planlarını (oluşturma, güncelleme, silme) yönetebildiği endpoint'lerdir.
3.  **Kullanıcı Üyelik Yönetimi (User-Specific):** Giriş yapmış kullanıcıların kendi mevcut üyeliklerini görmesi, üyelik geçmişini incelemesi, yeni bir plana abone olması (`create-checkout-session`) veya mevcut aboneliğini iptal etmesi için kullanılır.

---

## 1. Abonelik Planları (Herkese Açık)

### 1.1. Tüm Aktif Abonelik Planlarını Getir

Sistemdeki tüm aktif ve satın alınabilir üyelik planlarını listeler.

*   **Endpoint:** `GET /Membership/plans`
*   **Yetkilendirme:** Gerekmez (`AllowAnonymous`).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `PlanFeatureDto` objelerinden oluşan bir dizi.
    ```json
    [
        {
            "id": 1,
            "name": "Free",
            "description": "Temel ve giriş seviye için.",
            "price": 0,
            "currency": "USD",
            "billingCycle": "Monthly",
            // ... diğer plan özellikleri
        },
        {
            "id": 2,
            "name": "Plus",
            "description": "Orta ve Orta üstü kullanıcılar içindir.",
            "price": 10,
            "currency": "USD",
            // ... diğer plan özellikleri
        }
    ]
    ```

### 1.2. Belirli Bir Abonelik Planını Getir

ID ile belirtilen tek bir aktif abonelik planının detaylarını getirir.

*   **Endpoint:** `GET /Membership/plan/{Id}`
*   **Yetkilendirme:** Gerekmez (`AllowAnonymous`).

---

## 2. Yönetici Fonksiyonları (Admin Rolü Gerekli)

### 2.1. Yeni Abonelik Planı Oluştur

Sisteme yeni bir üyelik planı ekler.

*   **Endpoint:** `POST /Membership/plan`
*   **Yetkilendirme:** Gerekli (`Admin` rolü).

#### Request Body (`PlanFeatureCreateDto`)
| Alan | Tip | Açıklama |
| :--- | :--- | :--- |
| `planName` | `string` | Planın adı (örn: "Pro"). |
| `price` | `number` | Planın fiyatı. |
| `currency` | `string` | Para birimi (örn: "USD"). |
| `billingCycle`|`string`| Fatura döngüsü (örn: "Monthly", "Yearly").|
| `isActive` | `boolean`| Planın satın alınabilir olup olmadığı. |
| ... | ... | Diğer özellikler (`reporting`, `budgeting` vb.) |

#### Başarılı Yanıt (Success Response)
*   `201 Created` durum kodu ve oluşturulan planın objesi.

### 2.2. Abonelik Planını Güncelle

Mevcut bir üyelik planının detaylarını günceller.

*   **Endpoint:** `PUT /Membership/plan/{Id}`
*   **Yetkilendirme:** Gerekli (`Admin` rolü).

### 2.3. Abonelik Planını Sil

Mevcut bir üyelik planını sistemden kaldırır.

*   **Endpoint:** `DELETE /Membership/plan/{Id}`
*   **Yetkilendirme:** Gerekli (`Admin` rolü).

---

## 3. Kullanıcı Üyelik Yönetimi (Giriş Yapmış Kullanıcı)

### 3.1. Aktif Üyeliği Getir

Giriş yapmış kullanıcının mevcut ve aktif olan üyeliğini getirir.

*   **Endpoint:** `GET /Membership/current`
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

### 3.2. Üyelik Geçmişini Getir

Kullanıcının geçmiş ve mevcut tüm üyeliklerini listeler.

*   **Endpoint:** `GET /Membership/history`
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

### 3.3. Ödeme Oturumu Oluştur (Stripe Entegrasyonu)

Kullanıcının seçtiği bir plana abone olması için Stripe ödeme sayfasını başlatan oturumu oluşturur.

*   **Endpoint:** `POST /Membership/create-checkout-session`
*   **Açıklama:** Bu endpoint, bir plan ID'si alır, veritabanında `PendingPayment` (Ödeme Bekleniyor) durumunda bir üyelik kaydı oluşturur ve Stripe API'sine bağlanarak bir ödeme oturumu başlatır. Yanıt olarak, kullanıcının yönlendirileceği Stripe ödeme sayfasının URL'sini döner.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`SubscriptionRequestDto`)
| Alan | Tip | Açıklama |
| :--- | :--- | :--- |
| `planId` | `integer` | Abone olunmak istenen planın ID'si. |
| `autoRenew`| `boolean`| Aboneliğin otomatik olarak yenilenip yenilenmeyeceği. |

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "sessionId": "cs_test_a1B2c3D4...",
      "checkoutUrl": "https://checkout.stripe.com/c/pay/cs_test_a1B2c3D4..."
    }
    ```

### 3.4. Aboneliği İptal Et

Kullanıcının aktif bir aboneliğinin otomatik yenilenmesini durdurur.

*   **Endpoint:** `POST /Membership/{userMembershipId}/cancel`
*   **Açıklama:** Aboneliğin durumunu `Cancelled` olarak günceller. Üyelik, mevcut fatura döneminin sonuna kadar aktif kalmaya devam eder.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).