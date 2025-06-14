# FinTrack API: Kategori Yönetimi (Categories Controller)

Bu doküman, kullanıcıların gelir ve gider kategorilerini yönetmek için kullanılan `Categories` endpoint'lerini açıklamaktadır.

**Controller Base Path:** `/api/categories`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir `Bearer Token` gönderilmelidir. Token'a sahip kullanıcının rolü `User` veya `Admin` olmalıdır.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

Hatalı veya eksik token durumunda `401 Unauthorized` hatası döner.

---

## Endpoints

### 1. Kullanıcının Tüm Kategorilerini Getir

Giriş yapmış kullanıcının sistemde kayıtlı tüm gelir/gider kategorilerini listeler.

*   **Endpoint:** `GET /api/categories`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm kategorilerin bir listesini döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `CategoryModel` objelerinden oluşan bir dizi.
    ```json
    [
      {
        "categoryId": 1,
        "userId": 22,
        "name": "Maaş",
        "type": 1
      },
      {
        "categoryId": 2,
        "userId": 22,
        "name": "Faturalar",
        "type": 0
      },
      {
        "categoryId": 3,
        "userId": 22,
        "name": "Market Alışverişi",
        "type": 0
      }
    ]
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Açıklama:** Kullanıcıya ait hiç kategori bulunamadığında döner.
*   **Status Code:** `500 Internal Server Error`
*   **Açıklama:** Sunucu tarafında beklenmedik bir hata oluştuğunda döner.

---

### 2. Belirli Bir Kategoriyi Getir

Kullanıcıya ait tek bir kategorinin detaylarını ID ile getirir.

*   **Endpoint:** `GET /api/categories/{categoryId}`
*   **Açıklama:** Verilen `categoryId`'ye sahip olan ve token'ı gönderen kullanıcıya ait olan kategorinin detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri

| Parametre    | Tip     | Açıklama                         | Zorunlu mu? |
|--------------|---------|----------------------------------|-------------|
| `categoryId` | `integer` | Detayı istenen kategorinin ID'si. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `CategoryModel` objesi.
    ```json
    {
      "categoryId": 2,
      "userId": 22,
      "name": "Faturalar",
      "type": 0
    }
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Açıklama:** Belirtilen `categoryId` ile bir kategori bulunamadığında veya bulunan kategori kullanıcıya ait olmadığında döner.
*   **Status Code:** `500 Internal Server Error`

---

### 3. Yeni Kategori Oluştur

Kullanıcı için yeni bir kategori oluşturur.

*   **Endpoint:** `POST /api/categories`
*   **Açıklama:** Gönderilen bilgilere göre yeni bir kategori oluşturur ve oluşturulan kaynağı döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`CategoryCreateDto`)

*   **Content-Type:** `application/json`

| Alan   | Tip      | Açıklama                                   | Zorunlu mu? |
|--------|----------|--------------------------------------------|-------------|
| `name` | `string` | Kategorinin adı (örn: "Kira").             | Evet        |
| `type` | `integer`| Kategorinin türü. `0` = Gider, `1` = Gelir. | Evet        |

#### Request Body Örneği

```json
{
  "name": "Ulaşım",
  "type": 0
}
```

#### Başarılı Cevap (Success Response)

*   **Status Code:** `201 Created`
*   **Headers:**
    *   `Location`: `/api/categories/{yeni_kategori_id}` (Oluşturulan kaynağın URL'si)
*   **Content:** Oluşturulan `CategoryModel` objesi.
    ```json
    {
        "categoryId": 4,
        "userId": 22,
        "name": "Ulaşım",
        "type": 0
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `400 Bad Request` (Eksik veya hatalı veri gönderildiğinde)
*   **Status Code:** `500 Internal Server Error`

---

### 4. Kategoriyi Güncelle

Mevcut bir kategoriyi günceller.

*   **Endpoint:** `PUT /api/categories/{categoryId}`
*   **Açıklama:** Belirtilen `categoryId`'ye sahip kategoriyi, gönderilen verilerle günceller.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri
| Parametre    | Tip     | Açıklama                           | Zorunlu mu? |
|--------------|---------|------------------------------------|-------------|
| `categoryId` | `integer` | Güncellenecek kategorinin ID'si.   | Evet        |

#### Request Body (`CategoryUpdateDto`)

| Alan   | Tip      | Açıklama                                   | Zorunlu mu? |
|--------|----------|--------------------------------------------|-------------|
| `name` | `string` | Kategorinin yeni adı.                      | Evet        |
| `type` | `integer`| Kategorinin yeni türü. `0`=Gider, `1`=Gelir.| Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Açıklama:** Güncelleme başarılı olduğunda, kategorinin güncellenmiş hali response body'de döner.
*   **Content:** Güncellenmiş `CategoryModel` objesi.
    ```json
    {
        "categoryId": 4,
        "userId": 22,
        "name": "Ulaşım Giderleri",
        "type": 0
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Kategori bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`

#### Örnek Kullanım (cURL)

---

### 5. Kategoriyi Sil

Mevcut bir kategoriyi siler.

*   **Endpoint:** `DELETE /api/categories/{categoryId}`
*   **Açıklama:** Belirtilen `categoryId`'ye sahip kategoriyi kalıcı olarak siler. Bu işlem geri alınamaz.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri
| Parametre    | Tip     | Açıklama                    | Zorunlu mu? |
|--------------|---------|-----------------------------|-------------|
| `categoryId` | `integer` | Silinecek kategorinin ID'si. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `204 No Content`
*   **Açıklama:** Silme işlemi başarılı olduğunda response body'de içerik dönmez.

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Kategori bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`