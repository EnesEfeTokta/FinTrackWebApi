# FinTrack API: İşlem Kategorileri Yönetimi (TransactionCategory Controller)

Bu doküman, kullanıcıların gelir ve gider işlemlerini sınıflandırmak için kullandıkları kişisel **işlem kategorilerini** yöneten `TransactionCategoryController` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/TransactionCategory`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir.

### Veri İzolasyonu (User Scoping)

Tüm kategori işlemleri, işlemi yapan kullanıcının kimliğine bağlıdır. Bir kullanıcı, yalnızca kendi oluşturduğu işlem kategorilerini listeleyebilir, görüntüleyebilir, güncelleyebilir veya silebilir.

### `type` Alanı Değerleri (`TransactionType`)

İşlem kategorileri, `Gelir` veya `Gider` olarak sınıflandırılır. Bu, kullanıcıların bütçeleme ve raporlama sırasında daha anlamlı gruplamalar yapmasına olanak tanır.
*   `Income` (Gelir)
*   `Expense` (Gider)

---

## Endpoints

### 1. Kullanıcının Tüm İşlem Kategorilerini Getir

Giriş yapmış kullanıcının sistemde kayıtlı tüm işlem kategorilerini listeler.

*   **Endpoint:** `GET /TransactionCategory`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm işlem kategorilerinin bir listesini döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `TransactionCategoriesDto` objelerinden oluşan bir dizi.
    ```json
    [
        {
          "id": 1,
          "name": "Maaş",
          "type": "Income",
          "createdAt": "2024-05-01T10:00:00Z",
          "updatedAt": null
        },
        {
          "id": 2,
          "name": "Faturalar",
          "type": "Expense",
          "createdAt": "2024-05-02T11:30:00Z",
          "updatedAt": "2024-05-22T14:00:00Z"
        }
    ]
    ```

#### Hata Yanıtları (Error Responses)
*   `404 Not Found`: Kullanıcının hiç kategorisi yoksa.
*   `500 Internal Server Error`

---

### 2. Belirli Bir İşlem Kategorisini Getir

Kullanıcıya ait tek bir işlem kategorisinin detaylarını ID ile getirir.

*   **Endpoint:** `GET /TransactionCategory/{Id}`
*   **Açıklama:** Verilen `Id`'ye sahip olan ve kullanıcıya ait olan işlem kategorisinin detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** Tek bir `TransactionCategoriesDto` objesi.

#### Hata Yanıtları (Error Responses)
*   `404 Not Found`
*   `500 Internal Server Error`

---

### 3. Yeni İşlem Kategorisi Oluştur

Kullanıcı için yeni bir işlem kategorisi oluşturur.

*   **Endpoint:** `POST /TransactionCategory`
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`TransactionCategoriesCreateDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | Kategorinin adı (örn: "Ulaşım"). | Evet |
| `type` | `string` | Kategorinin türü (`Income` veya `Expense`). | Evet |

#### Request Body Örneği
```json
{
  "name": "Kira",
  "type": "Expense"
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `201 Created`
*   **Content:** Oluşturulan `TransactionCategoryModel` objesi.

---

### 4. İşlem Kategorisini Güncelle

Mevcut bir işlem kategorisinin adını ve türünü günceller.

*   **Endpoint:** `PUT /TransactionCategory/{Id}`
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`TransactionCategoriesCreateDto` kullanılır)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | Kategorinin yeni adı. | Evet |
| `type` | `string` | Kategorinin yeni türü (`Income` veya `Expense`). | Evet |

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `true`

#### Hata Yanıtları (Error Responses)
*   `404 Not Found`
*   `500 Internal Server Error`

---

### 5. İşlem Kategorisini Sil

Mevcut bir işlem kategorisini siler.

*   **Endpoint:** `DELETE /TransactionCategory/{Id}`
*   **Açıklama:** Bu işlem geri alınamaz. Bu kategoriye bağlı işlemler varsa, bu işlemlerin kategorisiz kalabileceği dikkate alınmalıdır.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `true`

#### Hata Yanıtları (Error Responses)
*   `404 Not Found`
*   `500 Internal Server Error`