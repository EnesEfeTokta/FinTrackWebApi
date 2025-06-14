# FinTrack API: İşlem Yönetimi (Transactions Controller)

Bu doküman, kullanıcıların gelir ve gider gibi finansal işlemlerini yönetmek için kullanılan `Transactions` endpoint'lerini açıklamaktadır.

**Controller Base Path:** `/api/transactions`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir `Bearer Token` gönderilmelidir. Token'a sahip kullanıcının rolü `User` veya `Admin` olmalıdır.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

Hatalı veya eksik token durumunda `401 Unauthorized` hatası döner.

---

## Endpoints

### 1. Kullanıcının Tüm İşlemlerini Getir

Giriş yapmış kullanıcının sistemde kayıtlı tüm finansal işlemlerini, kategori ve hesap detaylarıyla birlikte listeler.

*   **Endpoint:** `GET /api/transactions`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm işlemleri döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `TransactionDto` objelerinden oluşan bir dizi.
    ```json
    [
      {
        "transactionId": 101,
        "categoryId": 5,
        "categoryName": "Maaş",
        "categoryType": 1, // 1: Income
        "userId": 15,
        "accountId": 1,
        "accountName": "Banka Hesabım",
        "amount": 25000.00,
        "transactionDateUtc": "2023-10-01T10:00:00Z",
        "description": "Ekim ayı maaş ödemesi"
      },
      {
        "transactionId": 102,
        "categoryId": 8,
        "categoryName": "Faturalar",
        "categoryType": 0, // 0: Expense
        "userId": 15,
        "accountId": 1,
        "accountName": "Banka Hesabım",
        "amount": 750.50,
        "transactionDateUtc": "2023-10-05T14:30:00Z",
        "description": "Elektrik ve su faturası"
      }
    ]
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `500 Internal Server Error`

---

### 2. Belirli Bir İşlemi Getir

Tek bir finansal işlemin detaylarını ID ile getirir.

*   **Endpoint:** `GET /api/transactions/{transactionId}`
*   **Açıklama:** Verilen `transactionId`'ye sahip olan ve kullanıcıya ait olan işlemin detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri

| Parametre       | Tip     | Açıklama                      | Zorunlu mu? |
|-----------------|---------|-------------------------------|-------------|
| `transactionId` | `integer` | Detayı istenen işlemin ID'si. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `TransactionDto` objesi.
    ```json
    {
      "transactionId": 102,
      "categoryId": 8,
      "categoryName": "Faturalar",
      "categoryType": 0,
      "userId": 15,
      "accountId": 1,
      "accountName": "Banka Hesabım",
      "amount": 750.50,
      "transactionDateUtc": "2023-10-05T14:30:00Z",
      "description": "Elektrik ve su faturası"
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 3. İşlemleri Kategori Türüne Göre Filtrele

Kullanıcının işlemlerini kategori türüne göre (`Income` veya `Expense`) filtreleyerek listeler.

*   **Endpoint:** `GET /api/transactions/category-type/{type}`
*   **Açıklama:** Sadece belirtilen türdeki işlemleri döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri

| Parametre | Tip      | Açıklama                                       | Zorunlu mu? |
|-----------|----------|------------------------------------------------|-------------|
| `type`    | `string` | Filtrelenecek kategori türü: `Income` veya `Expense`. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** Belirtilen türe ait `TransactionDto` objelerinden oluşan bir dizi.

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Belirtilen türde hiç işlem yoksa).
*   **Status Code:** `500 Internal Server Error`

---

### 4. İşlemleri Kategori Adına Göre Filtrele

Kullanıcının işlemlerini tam kategori adına göre filtreleyerek listeler.

*   **Endpoint:** `GET /api/transactions/category-name/{category}`
*   **Açıklama:** Sadece belirtilen kategoriye ait işlemleri döndürür. Eşleşme birebir olmalıdır.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri

| Parametre  | Tip      | Açıklama                                       | Zorunlu mu? |
|------------|----------|------------------------------------------------|-------------|
| `category` | `string` | Filtrelenecek kategorinin adı (örn: "Faturalar"). | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** Belirtilen kategoriye ait `TransactionDto` objelerinden oluşan bir dizi.

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Belirtilen kategoride hiç işlem yoksa).
*   **Status Code:** `500 Internal Server Error`

---

### 5. Yeni İşlem Oluştur

Kullanıcı için yeni bir finansal işlem oluşturur.

*   **Endpoint:** `POST /api/transactions`
*   **Açıklama:** Yeni bir gelir veya gider kaydı oluşturur.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`TransactionCreateDto`)
**Not:** `UserId` alanı DTO'da zorunlu olsa da, bu değer dikkate alınmaz ve her zaman token'dan gelen kullanıcı kimliği kullanılır.

| Alan                 | Tip      | Açıklama                               | Zorunlu mu? |
|----------------------|----------|----------------------------------------|-------------|
| `userId`             | `integer`| (Gönderilmeli fakat kullanılmıyor)     | Evet        |
| `categoryId`         | `integer`| İşlemin ait olduğu kategori ID'si.     | Evet        |
| `accountId`          | `integer`| İşlemin yapıldığı hesap ID'si.         | Evet        |
| `amount`             | `number` | İşlem tutarı.                          | Evet        |
| `transactionDateUtc` | `string` | İşlem tarihi (ISO 8601 formatı, UTC).  | Evet        |
| `description`        | `string` | İşlem açıklaması.                      | Evet        |

#### Request Body Örneği

```json
{
  "userId": 0,
  "categoryId": 10,
  "accountId": 2,
  "amount": 120.00,
  "transactionDateUtc": "2023-10-28T18:00:00Z",
  "description": "Akşam yemeği"
}
```

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** İşlemin başarıyla oluşturulduğunu belirten bir mesaj.
    ```json
    {
      "transactionId": 103,
      "message": "Transaction created successfully."
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `400 Bad Request` (Geçersiz veya eksik veri, bulunamayan `categoryId` vb.).
*   **Status Code:** `500 Internal Server Error`

---

### 6. İşlemi Güncelle

Mevcut bir finansal işlemi günceller.

*   **Endpoint:** `PUT /api/transactions/{transactionId}`
*   **Açıklama:** Belirtilen `transactionId`'ye sahip işlemi, gönderilen verilerle günceller.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri
| Parametre       | Tip     | Açıklama                     | Zorunlu mu? |
|-----------------|---------|------------------------------|-------------|
| `transactionId` | `integer` | Güncellenecek işlemin ID'si. | Evet        |

#### Request Body (`TransactionUpdateDto`)
* `POST` metodundaki ile benzer yapıdadır, `userId` alanı yoktur.

#### Başarılı Cevap (Success Response)
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "message": "Transaction updated successfully."
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `400 Bad Request`
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 7. İşlemi Sil

Mevcut bir finansal işlemi siler.

*   **Endpoint:** `DELETE /api/transactions/{transactionId}`
*   **Açıklama:** Belirtilen `transactionId`'ye sahip işlemi kalıcı olarak siler.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri
| Parametre       | Tip     | Açıklama                 | Zorunlu mu? |
|-----------------|---------|--------------------------|-------------|
| `transactionId` | `integer` | Silinecek işlemin ID'si. | Evet        |

#### Başarılı Cevap (Success Response)
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "message": "Transaction deleted successfully."
    }
    ```
#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`