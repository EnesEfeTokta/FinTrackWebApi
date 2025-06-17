# FinTrack API: Bütçe Kalemi Yönetimi (BudgetCategory Controller)

Bu doküman, bir bütçeye ait kategorileri ve bu kategorilere ayrılan tutarları (bütçe kalemlerini) yönetmek için kullanılan `BudgetCategory` endpoint'lerini açıklamaktadır.

**Controller Base Path:** `/api/budgetcategory`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir `Bearer Token` gönderilmelidir. Herhangi bir role sahip, giriş yapmış bir kullanıcı bu endpoint'leri kullanabilir.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

Hatalı veya eksik token durumunda `401 Unauthorized` hatası döner.

---

## Endpoints

### 1. Kullanıcının Tüm Bütçe Kalemlerini Getir

Giriş yapmış kullanıcının, tüm bütçelerine dağıtılmış bütün kategori kalemlerini listeler.

*   **Endpoint:** `GET /api/budgetcategory`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm bütçe-kategori atamalarını döndürür.
*   **Yetkilendirme:** Gerekli.

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `BudgetCategoryModel` objelerinden oluşan bir dizi.
    ```json
    [
      {
        "budgetCategoryId": 10,
        "budgetId": 1,
        "categoryId": 5,
        "allocatedAmount": 2500.00
      },
      {
        "budgetCategoryId": 11,
        "budgetId": 1,
        "categoryId": 8,
        "allocatedAmount": 750.50
      },
      {
        "budgetCategoryId": 12,
        "budgetId": 2,
        "categoryId": 12,
        "allocatedAmount": 5000.00
      }
    ]
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Açıklama:** Kullanıcıya ait hiç bütçe kalemi bulunamadığında döner.
*   **Status Code:** `500 Internal Server Error`

---

### 2. Belirli Bir Bütçe Kalemini Getir

Tek bir bütçe kaleminin detaylarını ID ile getirir.

*   **Endpoint:** `GET /api/budgetcategory/{id}`
*   **Açıklama:** Verilen `id`'ye sahip olan ve token'ı gönderen kullanıcıya ait olan bütçe kaleminin detaylarını döndürür.
*   **Yetkilendirme:** Gerekli.

#### URL Parametreleri

| Parametre | Tip     | Açıklama                           | Zorunlu mu? |
|-----------|---------|------------------------------------|-------------|
| `id`      | `integer` | Detayı istenen bütçe kalemi ID'si. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `BudgetCategoryModel` objesi.
    ```json
    {
      "budgetCategoryId": 10,
      "budgetId": 1,
      "categoryId": 5,
      "allocatedAmount": 2500.00
    }
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Açıklama:** Belirtilen `id` ile bir bütçe kalemi bulunamadığında veya bulunan kalem kullanıcıya ait bir bütçeye dahil olmadığında döner.
*   **Status Code:** `500 Internal Server Error`

---

### 3. Yeni Bütçe Kalemi Oluştur

Mevcut bir bütçeye yeni bir kategori ve bu kategori için bir harcama limiti ekler.

*   **Endpoint:** `POST /api/budgetcategory`
*   **Açıklama:** Bir bütçe ile bir kategoriyi ilişkilendirir ve bu ilişki için bir tutar belirler.
*   **Yetkilendirme:** Gerekli.

#### Request Body (`BudgetCategoryCreateDto`)

*   **Content-Type:** `application/json`

| Alan            | Tip      | Açıklama                                       | Zorunlu mu? |
|-----------------|----------|------------------------------------------------|-------------|
| `budgetId`      | `integer`| Kalemin ekleneceği bütçenin ID'si.             | Evet        |
| `categoryId`    | `integer`| Bütçeye eklenecek kategorinin ID'si.           | Evet        |
| `allocatedAmount` | `number` | Bu kategori için bütçeden ayrılan tutar.       | Evet        |

#### Request Body Örneği

```json
{
  "budgetId": 1,
  "categoryId": 9,
  "allocatedAmount": 300.00
}
```

#### Başarılı Cevap (Success Response)

*   **Status Code:** `201 Created`
*   **Headers:**
    *   `Location`: `/api/budgetcategory/{yeni_kalem_id}` (Oluşturulan kaynağın URL'si)
*   **Content:** Oluşturulan `BudgetCategoryModel` objesi.
    ```json
    {
      "budgetCategoryId": 13,
      "budgetId": 1,
      "categoryId": 9,
      "allocatedAmount": 300.00
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Gönderilen `budgetId` kullanıcıya ait değilse veya bulunamıyorsa).
*   **Status Code:** `500 Internal Server Error`

---

### 4. Bütçe Kalemini Güncelle

Mevcut bir bütçe kaleminin ayrılan tutarını günceller.

*   **Endpoint:** `PUT /api/budgetcategory/{id}`
*   **Açıklama:** Belirtilen `id`'ye sahip bütçe kaleminin sadece `allocatedAmount` değerini günceller.
*   **Yetkilendirme:** Gerekli.

#### URL Parametreleri
| Parametre | Tip     | Açıklama                             | Zorunlu mu? |
|-----------|---------|--------------------------------------|-------------|
| `id`      | `integer` | Güncellenecek bütçe kaleminin ID'si. | Evet        |

#### Request Body (`BudgetCategoryUpdateDto`)
**Not:** Bu DTO'da `BudgetId` ve `CategoryId` zorunlu olsa da, bu endpoint sadece `allocatedAmount` değerini güncellemek için kullanır.

| Alan            | Tip      | Açıklama                             | Zorunlu mu? |
|-----------------|----------|--------------------------------------|-------------|
| `budgetId`      | `integer`| (Gönderilmesi zorunlu, kullanılmıyor) | Evet        |
| `categoryId`    | `integer`| (Gönderilmesi zorunlu, kullanılmıyor) | Evet        |
| `allocatedAmount` | `number` | Bu kategori için ayrılan yeni tutar.   | Evet        |

#### Request Body Örneği

```json
{
  "budgetId": 1,
  "categoryId": 9,
  "allocatedAmount": 350.75
}
```

#### Başarılı Cevap (Success Response)

*   **Status Code:** `204 No Content`
*   **Açıklama:** Güncelleme başarılı olduğunda response body'de içerik dönmez.

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Bütçe kalemi bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`

---

### 5. Bütçe Kalemini Sil

Mevcut bir bütçe kalemini siler.

*   **Endpoint:** `DELETE /api/budgetcategory/{id}`
*   **Açıklama:** Belirtilen `id`'ye sahip bütçe kalemini kalıcı olarak siler.
*   **Yetkilendirme:** Gerekli.

#### URL Parametreleri
| Parametre | Tip     | Açıklama                         | Zorunlu mu? |
|-----------|---------|----------------------------------|-------------|
| `id`      | `integer` | Silinecek bütçe kaleminin ID'si. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `204 No Content`
*   **Açıklama:** Silme işlemi başarılı olduğunda response body'de içerik dönmez.

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Bütçe kalemi bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`