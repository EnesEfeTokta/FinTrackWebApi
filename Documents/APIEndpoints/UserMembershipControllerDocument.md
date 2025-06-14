# FinTrack API: Üyelik Yönetimi (Membership Controller)

Bu doküman, kullanıcıların üyelik planlarını yönetmesi, yeni üyelik başlatması ve mevcut üyeliklerini iptal etmesi için kullanılan `Membership` endpoint'lerini açıklamaktadır.

**Controller Base Path:** `/api/membership`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir `Bearer Token` gönderilmelidir. Token'a sahip kullanıcının rolü **`Admin`** olmalıdır.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

Hatalı veya eksik token durumunda `401 Unauthorized` hatası döner.

---

## Endpoints

### 1. Aktif Üyeliği Getir

Giriş yapmış kullanıcının mevcut, aktif olan ve bitiş tarihi gelecekte olan üyeliğini getirir.

*   **Endpoint:** `GET /api/membership/current`
*   **Açıklama:** Kullanıcının birden fazla aktif üyeliği varsa, bitiş tarihi en ileride olanı döndürür. Aktif üyelik yoksa `404 Not Found` döner.
*   **Yetkilendirme:** Gerekli (`Admin` rolü).

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `UserMembershipDto` objesi.
    ```json
    {
      "userMembershipId": 101,
      "planId": 2,
      "planName": "Premium Yıllık",
      "startDate": "2023-01-15T10:00:00Z",
      "endDate": "2024-01-15T10:00:00Z",
      "status": "Active",
      "autoRenew": true
    }
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Açıklama:** Kullanıcıya ait aktif bir üyelik bulunamadığında döner.
*   **Status Code:** `500 Internal Server Error`

---

### 2. Üyelik Geçmişini Getir

Kullanıcının geçmiş ve mevcut tüm üyeliklerini (aktif, iptal edilmiş, süresi dolmuş vb.) listeler.

*   **Endpoint:** `GET /api/membership/history`
*   **Açıklama:** Üyelikleri başlangıç tarihine göre en yeniden eskiye doğru sıralanmış olarak döndürür.
*   **Yetkilendirme:** Gerekli (`Admin` rolü).

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `UserMembershipDto` objelerinden oluşan bir dizi.
    ```json
    [
      {
        "userMembershipId": 101,
        "planId": 2,
        "planName": "Premium Yıllık",
        "startDate": "2023-01-15T10:00:00Z",
        "endDate": "2024-01-15T10:00:00Z",
        "status": "Active",
        "autoRenew": true
      },
      {
        "userMembershipId": 55,
        "planId": 1,
        "planName": "Standart Aylık",
        "startDate": "2022-12-15T09:00:00Z",
        "endDate": "2023-01-15T09:00:00Z",
        "status": "Expired",
        "autoRenew": false
      }
    ]
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `500 Internal Server Error`

---

### 3. Ödeme Oturumu Oluştur (Yeni Üyelik Başlat)

Kullanıcı için yeni bir üyelik başlatır. Planın ücretli veya ücretsiz olmasına göre farklı davranır.

*   **Endpoint:** `POST /api/membership/create-checkout-session`
*   **Açıklama:**
    *   **Ücretli Planlar:** Bir ödeme oturumu (Stripe vb.) oluşturur ve kullanıcıyı ödeme sayfasına yönlendirmek için bir URL döndürür. Üyelik başlangıçta `PendingPayment` durumundadır.
    *   **Ücretsiz Planlar:** Üyeliği anında `Active` duruma getirir ve ödeme işlemi gerektirmez.
*   **Yetkilendirme:** Gerekli (`Admin` rolü).

#### Request Body (`SubscriptionRequestDto`)

| Alan        | Tip      | Açıklama                                       | Zorunlu mu? |
|-------------|----------|------------------------------------------------|-------------|
| `planId`    | `integer`| Abone olunacak üyelik planının ID'si.          | Evet        |
| `autoRenew` | `boolean`| Üyeliğin dönem sonunda otomatik yenilenip yenilenmeyeceği. | Hayır (Varsayılan: `true`) |

#### Request Body Örneği

```json
{
  "planId": 2,
  "autoRenew": true
}
```

#### Başarılı Cevap - A (Ücretli Plan)

*   **Status Code:** `200 OK`
*   **Content:** Ödeme oturum ID'si ve yönlendirme URL'si.
    ```json
    {
      "sessionId": "cs_test_a1B2c3d4E5f6G7h8I9j0...",
      "checkoutUrl": "https://checkout.stripe.com/c/pay/cs_test_a1B2c3d4E5f6G7h8I9j0..."
    }
    ```

#### Başarılı Cevap - B (Ücretsiz Plan)

*   **Status Code:** `200 OK`
*   **Content:** Başarılı mesajı ve oluşturulan üyelik ID'si.
    ```json
    {
      "message": "Successfully subscribed to the free plan.",
      "userMembershipId": 102,
      "sessionId": null,
      "checkoutUrl": null
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `400 Bad Request` (Eksik veri, kullanıcı zaten bu plana aboneyse vb.)
*   **Status Code:** `404 Not Found` (Belirtilen `planId` geçersiz veya aktif değilse.)
*   **Status Code:** `500 Internal Server Error` (Ödeme sağlayıcısıyla iletişimde hata oluşursa.)

---

### 4. Üyeliği İptal Et

Mevcut ve aktif bir üyeliğin otomatik yenilenmesini durdurur.

*   **Endpoint:** `POST /api/membership/{userMembershipId}/cancel`
*   **Açıklama:** Bu işlem üyeliği anında sonlandırmaz. Otomatik yenilemeyi kapatır ve üyelik `Status` alanını `Cancelled` olarak günceller. Üyelik, mevcut `endDate`'e kadar kullanılmaya devam eder.
*   **Yetkilendirme:** Gerekli (`Admin` rolü).

#### URL Parametreleri
| Parametre          | Tip     | Açıklama                           | Zorunlu mu? |
|--------------------|---------|------------------------------------|-------------|
| `userMembershipId` | `integer` | İptal edilecek üyeliğin ID'si.     | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** İptal işleminin başarılı olduğunu ve son geçerlilik tarihini belirten bir mesaj.
    ```json
    {
      "message": "Subscription cancellation requested. It will expire on 1/15/2024"
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Üyelik bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `400 Bad Request` (Sadece `Active` durumdaki üyelikler iptal edilebilir).
*   **Status Code:** `500 Internal Server Error`