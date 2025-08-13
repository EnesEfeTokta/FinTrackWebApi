# FinTrack API: Güvenli Borç Sistemi (Debt Controller)

Bu doküman, FinTrack'in yenilikçi **Güvenli Borç Sistemi'ni (GBS)** yöneten `DebtController` endpoint'lerini açıklamaktadır. GBS, kullanıcılar arasında video doğrulamalı ve yasal delil niteliği taşıyan güvenli bir borç alıp verme ortamı sağlar.

*Controller Base Path:* `/Debt`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir.

### Güvenli Borç Sistemi (GBS) İş Akışı

1.  **Teklif Oluşturma (`/create-debt-offer`):** "Alacaklı" (Lender), "Borçlu'nun" (Borrower) e-posta adresini kullanarak bir borç teklifi oluşturur.
2.  **Teklifi Yanıtlama (`/respond-to-offer/{debtId}`):** "Borçlu", gelen teklifi kabul eder veya reddeder.
    *   **Kabul Ederse:** Borcun durumu `AcceptedPendingVideoUpload` (Video Yüklemesi Bekleniyor) olarak güncellenir.
    *   **Reddederse:** Borcun durumu `RejectedByBorrower` (Borçlu Tarafından Reddedildi) olarak güncellenir ve süreç sonlanır.
3.  **Video Yükleme (Ayrı Servis):** Borçlu, bu aşamada güvenlik videosunu sisteme yükler. Bu işlem ayrı bir video yönetim servisi tarafından yönetilir.
4.  **Operatör Onayı (Yönetim Paneli):** Operatör, yüklenen videoyu ve borç detaylarını inceler. Onay verirse borcun durumu `Active` (Aktif) olur.
5.  **Temerrüde Düşürme (`/mark-as-defaulted/{debtId}`):** Borcun vadesi geçtiği halde ödenmezse, "Alacaklı" borcu "temerrüde düştü" (`Defaulted`) olarak işaretleyebilir. Bu işlem, alacaklının video deliline erişim hakkını aktive eder.

### `status` Alanı Değerleri (`DebtStatusType`)

API istek ve yanıtlarında `status` alanı aşağıdaki metin değerlerini alabilir:
*   `PendingBorrowerAcceptance`
*   `AcceptedPendingVideoUpload`
*   `PendingOperatorApproval`
*   `Active`
*   `Paid`
*   `RejectedByBorrower`
*   `RejectedByOperator`
*   `Defaulted`
*   `Cancelled`

---

## Endpoints

### 1. Kullanıcının Borçlarını Listele

Giriş yapmış kullanıcının "alacaklı" veya "borçlu" olduğu tüm borç kayıtlarını listeler.

*   **Endpoint:** `GET /Debt`
*   **Açıklama:** Kullanıcının dahil olduğu tüm borçların detaylı bir listesini döndürür.
*   **Yetkilendirme:** Gerekli.

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `DebtDto` objelerinden oluşan bir dizi.
    ```json
    [
        {
          "id": 1,
          "lenderId": 15,
          "lenderName": "Ali_Veli",
          "borrowerId": 22,
          "borrowerName": "Ayse_Yilmaz",
          "amount": 2500,
          "currency": "TRY",
          "dueDateUtc": "2024-07-15T00:00:00Z",
          "description": "Acil ihtiyaç için",
          "status": "Active",
          "videoMetadataId": "guid-for-video...",
          // ... diğer tarih ve kullanıcı detay alanları
        }
    ]
    ```

---

### 2. Tek Bir Borcun Detayını Getir

ID ile belirtilen tek bir borcun tüm detaylarını getirir.

*   **Endpoint:** `GET /Debt/{Id}`
*   **Açıklama:** Verilen `Id`'ye sahip borcun detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (Kullanıcı borcun taraflarından biri olmalıdır).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** Tek bir `DebtDto` objesi. 
    ```json
    [
        {
          "id": 1,
          "lenderId": 15,
          "lenderName": "Ali_Veli",
          "borrowerId": 22,
          "borrowerName": "Ayse_Yilmaz",
          "amount": 2500,
          "currency": "TRY",
          "dueDateUtc": "2024-07-15T00:00:00Z",
          "description": "Acil ihtiyaç için",
          "status": "Active",
          "videoMetadataId": "guid-for-video...",
          // ... diğer tarih ve kullanıcı detay alanları
        }
    ]
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `404 Not Found`

---

### 3. Borç Teklifi Oluştur (Adım 1)

Alacaklının, bir borçluya yeni bir borç teklifi göndermesini sağlar.

*   **Endpoint:** `POST /Debt/create-debt-offer`
*   **Açıklama:** Yeni bir borç kaydı oluşturur ve durumunu `PendingBorrowerAcceptance` olarak ayarlar.
*   **Yetkilendirme:** Gerekli.

#### Request Body (`CreateDebtOfferRequestDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `borrowerEmail` | `string` | Borç verilecek kullanıcının e-posta adresi. | Evet |
| `amount` | `number` | Borç miktarı. | Evet |
| `currencyCode` | `string` | Para birimi (örn: "TRY", "USD"). | Evet |
| `dueDateUtc` | `string` | Son ödeme tarihi (ISO 8601). | Evet |
| `description` | `string` | Borç hakkında kısa açıklama. | Hayır |

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
    ```json
    {
      "success": true,
      "message": "Debt offer created successfully. Waiting for borrower's approval.",
      "debtId": 12
    }
    ```

---

### 4. Borç Teklifini Yanıtla (Adım 2)

Borçlunun, kendisine gelen bir borç teklifini kabul veya reddetmesini sağlar.

*   **Endpoint:** `POST /Debt/respond-to-offer/{debtId}`
*   **Açıklama:** Sadece borçlu olan kullanıcı bu endpoint'i çağırabilir. Borcun durumunu günceller.
*   **Yetkilendirme:** Gerekli (Sadece borçlu olan kullanıcı).

#### Request Body (`RespondToOfferRequestDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `accepted` | `boolean` | `true` ise teklif kabul edilir, `false` ise reddedilir. | Evet |

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `true`

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `403 Forbidden`: İşlemi yapmaya çalışan kullanıcı, borcun borçlusu değilse.
*   **Status Code:** `400 Bad Request`: Teklif, yanıtlanabilecek bir durumda değilse (örn: `status` alanı `PendingBorrowerAcceptance` değilse).

---

### 5. Borcu Temerrüde Düşür (Adım 5)

Vadesi geçmiş bir borcun, alacaklı tarafından "Temerrüde Düştü" olarak işaretlenmesini sağlar.

*   **Endpoint:** `POST /Debt/mark-as-defaulted/{debtId}`
*   **Açıklama:** Sadece alacaklı olan kullanıcı bu endpoint'i çağırabilir. Borcun vadesinin geçmiş olması ve durumunun `Active` olması gerekir.
*   **Yetkilendirme:** Gerekli (Sadece alacaklı olan kullanıcı).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
    ```json
    {
      "success": true,
      "message": "Debt has been successfully marked as defaulted. You can now access the video evidence."
    }
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `403 Forbidden`: İşlemi yapmaya çalışan kullanıcı, borcun alacaklısı değilse.
*   **Status Code:** `400 Bad Request`: Borcun vadesi henüz geçmemişse veya `status` alanı `Active` değilse.