# FinTrack API: Bütçe Yönetimi (Budgets Controller)

Bu doküman, kullanıcıların bütçelerini ve bütçe kalemlerini yönetmek için kullanılan `Budgets` endpoint'lerini açıklamaktadır.

**Controller Base Path:** `/api/budgets`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir `Bearer Token` gönderilmelidir. Token'a sahip kullanıcının rolü `User` veya `Admin` olmalıdır.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

Hatalı veya eksik token durumunda `401 Unauthorized` hatası döner.

---

## Endpoints

### 1. Kullanıcının Tüm Bütçelerini Getir

Giriş yapmış kullanıcının sistemde kayıtlı tüm bütçelerini listeler.

*   **Endpoint:** `GET /api/budgets`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm bütçelerin bir listesini döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `BudgetModel` objelerinden oluşan bir dizi.
    ```json
    [
      {
        "budgetId": 1,
        "userId": 15,
        "name": "Ekim 2023 Aylık Giderleri",
        "description": "Aylık mutfak, fatura ve ulaşım giderleri için bütçe.",
        "startDate": "2023-10-01T00:00:00Z",
        "endDate": "2023-10-31T23:59:59Z",
        "isActive": true,
        "createdAtUtc": "2023-09-28T10:00:00Z",
        "updatedAtUtc": "2023-10-15T14:30:00Z"
      },
      {
        "budgetId": 2,
        "userId": 15,
        "name": "Yaz Tatili Fonu",
        "description": "Gelecek yaz tatili için birikim bütçesi.",
        "startDate": "2023-09-01T00:00:00Z",
        "endDate": "2024-06-01T00:00:00Z",
        "isActive": true,
        "createdAtUtc": "2023-09-01T11:00:00Z",
        "updatedAtUtc": null
      }
    ]
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Açıklama:** Kullanıcıya ait hiç bütçe bulunamadığında döner.
*   **Status Code:** `500 Internal Server Error`
*   **Açıklama:** Sunucu tarafında beklenmedik bir hata oluştuğunda döner.

---

### 2. Belirli Bir Bütçeyi Getir

Kullanıcıya ait tek bir bütçenin detaylarını ID ile getirir.

*   **Endpoint:** `GET /api/budgets/{id}`
*   **Açıklama:** Verilen `id`'ye sahip olan ve token'ı gönderen kullanıcıya ait olan bütçenin detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri

| Parametre | Tip     | Açıklama                      | Zorunlu mu? |
|-----------|---------|-------------------------------|-------------|
| `id`      | `integer` | Detayı istenen bütçenin ID'si. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `BudgetModel` objesi.
    ```json
    {
      "budgetId": 1,
      "userId": 15,
      "name": "Ekim 2023 Aylık Giderleri",
      "description": "Aylık mutfak, fatura ve ulaşım giderleri için bütçe.",
      "startDate": "2023-10-01T00:00:00Z",
      "endDate": "2023-10-31T23:59:59Z",
      "isActive": true,
      "createdAtUtc": "2023-09-28T10:00:00Z",
      "updatedAtUtc": "2023-10-15T14:30:00Z"
    }
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Açıklama:** Belirtilen `id` ile bir bütçe bulunamadığında veya bulunan bütçe kullanıcıya ait olmadığında döner.
*   **Status Code:** `500 Internal Server Error`

---

### 3. Yeni Bütçe Oluştur

Kullanıcı için yeni bir bütçe oluşturur.

*   **Endpoint:** `POST /api/budgets`
*   **Açıklama:** Gönderilen bilgilere göre yeni bir bütçe oluşturur ve oluşturulan kaynağı döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`BudgetDto`)

*   **Content-Type:** `application/json`

| Alan          | Tip      | Açıklama                                 | Zorunlu mu? |
|---------------|----------|------------------------------------------|-------------|
| `name`        | `string` | Bütçenin adı (örn: "Aylık Giderler").    | Evet        |
| `description` | `string` | Bütçe hakkında kısa açıklama.            | Hayır       |
| `startDate`   | `string` | Bütçenin başlangıç tarihi (YYYY-MM-DD).  | Evet        |
| `endDate`     | `string` | Bütçenin bitiş tarihi (YYYY-MM-DD).      | Evet        |
| `isActive`    | `boolean`| Bütçenin aktif olup olmadığı.            | Evet        |

#### Request Body Örneği

```json
{
  "name": "Kasım 2023 Bütçesi",
  "description": "Kasım ayı genel giderleri.",
  "startDate": "2023-11-01",
  "endDate": "2023-11-30",
  "isActive": true
}
```

#### Başarılı Cevap (Success Response)

*   **Status Code:** `201 Created`
*   **Headers:**
    *   `Location`: `/api/budgets/{yeni_bütçe_id}` (Oluşturulan kaynağın URL'si)
*   **Content:** Oluşturulan `BudgetModel` objesi.
    ```json
    {
        "budgetId": 3,
        "userId": 15,
        "name": "Kasım 2023 Bütçesi",
        "description": "Kasım ayı genel giderleri.",
        "startDate": "2023-11-01T00:00:00Z",
        "endDate": "2023-11-30T00:00:00Z",
        "isActive": true,
        "createdAtUtc": "2023-10-27T12:00:00Z",
        "updatedAtUtc": null
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `400 Bad Request` (Eksik veya hatalı veri gönderildiğinde)
*   **Status Code:** `500 Internal Server Error`

---

### 4. Bütçeyi Güncelle

Mevcut bir bütçeyi günceller.

*   **Endpoint:** `PUT /api/budgets/{id}`
*   **Açıklama:** Belirtilen `id`'ye sahip bütçeyi, gönderilen verilerle günceller.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri
| Parametre | Tip     | Açıklama                      | Zorunlu mu? |
|-----------|---------|-------------------------------|-------------|
| `id`      | `integer` | Güncellenecek bütçenin ID'si. | Evet        |

#### Request Body (`BudgetDto`)
*   Aynı `POST` metodundaki `BudgetDto` yapısı kullanılır.

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Açıklama:** Güncelleme başarılı olduğunda, bütçenin güncellenmiş hali response body'de döner.
*   **Content:** Güncellenmiş `BudgetModel` objesi.
    ```json
    {
        "budgetId": 1,
        "userId": 15,
        "name": "Ekim 2023 Giderleri (Revize)",
        "description": "Aylık mutfak, fatura ve ulaşım giderleri için bütçe.",
        "startDate": "2023-10-01T00:00:00Z",
        "endDate": "2023-10-31T23:59:59Z",
        "isActive": true,
        "createdAtUtc": "2023-09-28T10:00:00Z",
        "updatedAtUtc": "2023-10-27T13:00:00Z"
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Bütçe bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`

---

### 5. Bütçeyi Sil

Mevcut bir bütçeyi siler.

*   **Endpoint:** `DELETE /api/budgets/{id}`
*   **Açıklama:** Belirtilen `id`'ye sahip bütçeyi kalıcı olarak siler. Bu işlem geri alınamaz.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri
| Parametre | Tip     | Açıklama                 | Zorunlu mu? |
|-----------|---------|--------------------------|-------------|
| `id`      | `integer` | Silinecek bütçenin ID'si. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `204 No Content`
*   **Açıklama:** Silme işlemi başarılı olduğunda response body'de içerik dönmez.

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Bütçe bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`

---

### 6. Bir Bütçenin Kategorilerini Getir

Belirli bir bütçeye atanmış tüm bütçe kategorilerini ve limitlerini listeler.

*   **Endpoint:** `GET /api/budgets/categories/{id}`
*   **Açıklama:** `id`'si verilen bütçeye ait tüm bütçe-kategori ilişkilerini döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri
| Parametre | Tip     | Açıklama                                  | Zorunlu mu? |
|-----------|---------|-------------------------------------------|-------------|
| `id`      | `integer` | Kategorileri istenen bütçenin ID'si.      | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `BudgetCategory` (veya benzeri bir model) objelerinden oluşan bir dizi.
    ```json
    [
      {
        "budgetCategoryId": 20,
        "budgetId": 1,
        "categoryId": 5,
        "amount": 2500.00,
        "category": { "name": "Mutfak Giderleri" }
      },
      {
        "budgetCategoryId": 21,
        "budgetId": 1,
        "categoryId": 8,
        "amount": 750.00,
        "category": { "name": "Faturalar" }
      }
    ]
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Bütçe bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`