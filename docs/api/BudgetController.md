# FinTrack API: Bütçe Yönetimi (Budgets Controller)

Bu doküman, kullanıcıların finansal bütçelerini oluşturmak, takip etmek ve yönetmek için kullanılan `BudgetsController` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/Budgets`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir. Sistem, token içerisindeki `userId` üzerinden kullanıcıyı tanır ve işlemleri sadece o kullanıcı adına gerçekleştirir.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

### Dinamik Kategori Yönetimi

Bütçe oluşturma veya güncelleme sırasında, eğer belirtilen `category` adı kullanıcının mevcut kategorileri arasında yoksa, sistem bu kategoriyi kullanıcı için **otomatik olarak oluşturur**. Bu, kullanıcıların bütçe oluştururken anlık olarak yeni harcama kategorileri tanımlamasına olanak tanır.

---

## Endpoints

### 1. Kullanıcının Tüm Bütçelerini Getir

Giriş yapmış kullanıcının sistemde kayıtlı tüm bütçelerini listeler.

*   **Endpoint:** `GET /Budgets`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm bütçelerin bir listesini döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `BudgetDto` objelerinden oluşan bir dizi.
    ```json
    [
        {
          "id": 1,
          "name": "Aylık Market Alışverişi",
          "description": "Temel gıda ve temizlik malzemeleri için.",
          "category": "Market",
          "allocatedAmount": 5000.00,
          "reachedAmount": 2350.50,
          "currency": "TRY",
          "startDate": "2024-05-01T00:00:00Z",
          "endDate": "2024-05-31T23:59:59Z",
          "isActive": true,
          "createdAtUtc": "2024-05-01T10:00:00Z",
          "updatedAtUtc": "2024-05-20T15:00:00Z"
        }
    ]
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `500 Internal Server Error`

---

### 2. Belirli Bir Bütçeyi Getir

Kullanıcıya ait tek bir bütçenin detaylarını ID ile getirir.

*   **Endpoint:** `GET /Budgets/{id}`
*   **Açıklama:** Verilen `id`'ye sahip olan ve token'ı gönderen kullanıcıya ait olan bütçenin detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `BudgetDto` objesi.
    ```json
    {
      "id": 1,
      "name": "Aylık Market Alışverişi",
      "description": "Temel gıda ve temizlik malzemeleri için.",
      "category": "Market",
      "allocatedAmount": 5000.00,
      "reachedAmount": 2350.50,
      "currency": "TRY",
      "startDate": "2024-05-01T00:00:00Z",
      "endDate": "2024-05-31T23:59:59Z",
      "isActive": true,
      "createdAtUtc": "2024-05-01T10:00:00Z",
      "updatedAtUtc": "2024-05-20T15:00:00Z"
    }
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 3. Yeni Bütçe Oluştur

Kullanıcı için yeni bir finansal bütçe oluşturur.

*   **Endpoint:** `POST /Budgets`
*   **Açıklama:** Gönderilen bilgilere göre yeni bir bütçe oluşturur. Eğer belirtilen kategori mevcut değilse, onu da otomatik olarak oluşturur.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`BudgetCreateDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | Bütçenin adı. | Evet |
| `description`| `string` | Bütçe hakkında kısa açıklama. | Hayır |
| `category` | `string` | Bütçenin ilişkili olduğu kategori adı. | Evet |
| `allocatedAmount` | `number` | Bu bütçe için ayrılan toplam tutar. | Evet |
| `reachedAmount`| `number`| Bütçenin başlangıçtaki harcanan tutarı (genellikle 0).| Evet |
| `currency` | `string` | Para birimi (örn: "TRY", "USD"). | Evet |
| `startDate` | `string` | Bütçenin başlangıç tarihi (ISO 8601 formatında). | Evet |
| `endDate` | `string` | Bütçenin bitiş tarihi (ISO 8601 formatında). | Evet |
| `isActive` | `boolean` | Bütçenin aktif olup olmadığı. | Evet |

#### Request Body Örneği
```json
{
  "name": "Dışarıda Yemek Bütçesi",
  "category": "Restoran & Kafe",
  "allocatedAmount": 1500,
  "reachedAmount": 0,
  "currency": "TRY",
  "startDate": "2024-06-01T00:00:00Z",
  "endDate": "2024-06-30T23:59:59Z",
  "isActive": true
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `201 Created`
*   **Content:** Oluşturulan bütçenin `BudgetDto` objesi.

---

### 4. Bütçeyi Güncelle

Mevcut bir bütçeyi günceller.

*   **Endpoint:** `PUT /Budgets/{id}`
*   **Açıklama:** Belirtilen `id`'ye sahip bütçeyi, gönderilen verilerle günceller.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** Güncellenmiş bütçenin `BudgetDto` objesi.

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 5. Bütçenin Harcanan Tutarını Güncelle

Bir bütçenin sadece harcanan (`reachedAmount`) tutarını güncellemek için özel endpoint.

*   **Endpoint:** `PUT /Budgets/Update-Reached-Amount`
*   **Açıklama:** Genellikle bir işlem eklendiğinde veya silindiğinde, ilişkili bütçenin mevcut harcama miktarını güncellemek için kullanılır.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`BudgetUpdateReachedAmountDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `budgetId` | `integer`| Güncellenecek bütçenin ID'si. | Evet |
| `reachedAmount`|`number`| Bütçenin yeni harcanan toplam tutarı. | Evet |

#### Request Body Örneği
```json
{
  "budgetId": 1,
  "reachedAmount": 2500.75
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** Güncellenmiş bütçenin `BudgetDto` objesi.

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `400 Bad Request`
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 6. Bütçeyi Sil

Mevcut bir bütçeyi siler.

*   **Endpoint:** `DELETE /Budgets/{id}`
*   **Açıklama:** Belirtilen `id`'ye sahip bütçeyi kalıcı olarak siler. Bu işlem geri alınamaz.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `true`

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`