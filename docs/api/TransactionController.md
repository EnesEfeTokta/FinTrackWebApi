# FinTrack API: İşlem Yönetimi (Transactions Controller)

Bu doküman, kullanıcıların finansal işlemlerini (gelir/gider) kaydetmek, görüntülemek, güncellemek ve silmek için kullanılan `TransactionsController` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/Transactions`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir.

### Otomatik Bakiye Güncellemesi

Bu controller, bir işlem oluşturulduğunda veya silindiğinde, işlemin bağlı olduğu hesabın (`Account`) bakiyesini otomatik olarak günceller.
*   **İşlem Oluşturma:**
    *   `Income` (Gelir) türünde bir işlemse, hesap bakiyesi artırılır.
    *   `Expense` (Gider) türünde bir işlemse, hesap bakiyesi azaltılır.
*   **İşlem Silme:**
    *   `Income` (Gelir) türünde bir işlemse, hesap bakiyesi azaltılır (geri alınır).
    *   `Expense` (Gider) türünde bir işlemse, hesap bakiyesi artırılır (geri alınır).

---

## Endpoints

### 1. Kullanıcının Tüm İşlemlerini Getir

Giriş yapmış kullanıcının sistemdeki tüm işlemlerini listeler.

*   **Endpoint:** `GET /Transactions`
*   **Açıklama:** Kullanıcıya ait tüm gelir ve gider kayıtlarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `TransactionDto` objelerinden oluşan bir dizi.
    ```json
    [
        {
          "id": 1,
          "category": { "id": 1, "name": "Maaş", "type": "Income" /*...*/ },
          "account": { "id": 1, "name": "Maaş Hesabı", "balance": 15000 /*...*/ },
          "amount": 25000,
          "currency": "TRY",
          "transactionDateUtc": "2024-05-01T08:00:00Z",
          "description": "Mayıs Ayı Maaşı",
          "createdAtUtc": "2024-05-01T08:01:00Z",
          "updatedAtUtc": null
        }
    ]
    ```

### 2. Belirli Bir İşlemi Getir

Kullanıcıya ait tek bir işlemin detaylarını ID ile getirir.

*   **Endpoint:** `GET /Transactions/{Id}`
*   **Yetkilendirme:** Gerekli.

### 3. İşlemleri Kategori Türüne Göre Filtrele

Kullanıcının işlemlerini kategori türüne (`Income` veya `Expense`) göre filtreler.

*   **Endpoint:** `GET /Transactions/category-type/{type}`
*   **URL Parametresi:** `type` (string) - Değerleri: `Income`, `Expense`.
*   **Yetkilendirme:** Gerekli.

### 4. İşlemleri Kategori Adına Göre Filtrele

Kullanıcının işlemlerini belirli bir kategori adına göre filtreler.

*   **Endpoint:** `GET /Transactions/category-name/{category}`
*   **URL Parametresi:** `category` (string) - Kategorinin tam adı (örn: "Faturalar").
*   **Yetkilendirme:** Gerekli.

### 5. İşlemleri Hesaba Göre Filtrele

Kullanıcının işlemlerini belirli bir hesap ID'sine göre filtreler.

*   **Endpoint:** `GET /Transactions/account-id/{account}`
*   **URL Parametresi:** `account` (integer) - Hesabın ID'si.
*   **Yetkilendirme:** Gerekli.

### 6. Yeni İşlem Oluştur

Kullanıcı için yeni bir gelir veya gider kaydı oluşturur.

*   **Endpoint:** `POST /Transactions`
*   **Açıklama:** Bu işlem sonucunda, ilgili hesabın bakiyesi otomatik olarak güncellenir.
*   **Yetkilendirme:** Gerekli.

#### Request Body (`TransactionCreateDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `categoryId` | `integer`| İşlemin ilişkili olduğu kategori ID'si. | Evet |
| `accountId` | `integer`| İşlemin yapıldığı hesap ID'si. | Evet |
| `amount` | `number` | İşlem tutarı (Gider için negatif, gelir için pozitif).| Evet |
| `currency` | `string` | Para birimi (örn: "TRY"). Hesabın para birimi ile aynı olmalıdır. | Evet |
| `transactionDateUtc` | `string` | İşlemin yapıldığı tarih (ISO 8601). | Evet |
| `description` | `string` | İşlem hakkında açıklama. | Hayır |

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `201 Created`
*   **Content:** Oluşturulan `TransactionModel` objesi.

### 7. İşlemi Güncelle

Mevcut bir işlemin detaylarını günceller. **Not:** Bu işlem, hesap bakiyesini otomatik olarak **güncellemez**. Bakiye düzeltmeleri manuel olarak yapılmalıdır.

*   **Endpoint:** `PUT /Transactions/{Id}`
*   **Yetkilendirme:** Gerekli.

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `true`

### 8. İşlemi Sil

Mevcut bir işlemi siler.

*   **Endpoint:** `DELETE /Transactions/{Id}`
*   **Açıklama:** Bu işlem sonucunda, ilgili hesabın bakiyesi otomatik olarak güncellenir (işlem geri alınır).
*   **Yetkilendirme:** Gerekli.

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `true`