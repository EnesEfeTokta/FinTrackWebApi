# FinTrack API: Kategori Yönetimi (Categories Controller)

Bu doküman, kullanıcıların işlemlerini (gelir/gider) sınıflandırmak için kullandıkları kişisel kategorileri yöneten `CategoriesController` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/Categories`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

### Veri İzolasyonu (User Scoping)

Tüm kategori işlemleri, işlemi yapan kullanıcının kimliğine bağlıdır. Bir kullanıcı, yalnızca kendi oluşturduğu kategorileri listeleyebilir, görüntüleyebilir, güncelleyebilir veya silebilir. Başka bir kullanıcının verilerine erişim mümkün değildir.

---

## Endpoints

### 1. Kullanıcının Tüm Kategorilerini Getir

Giriş yapmış kullanıcının sistemde kayıtlı tüm harcama/gelir kategorilerini listeler.

*   **Endpoint:** `GET /Categories`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm kategorilerin bir listesini döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `CategoryDto` objelerinden oluşan bir dizi. Eğer kullanıcının hiç kategorisi yoksa, boş bir dizi `[]` döner.
    ```json
    [
        {
          "id": 1,
          "name": "Faturalar",
          "createdAtUtc": "2024-05-01T10:00:00Z",
          "updatedAtUtc": "2024-05-20T15:00:00Z"
        },
        {
          "id": 2,
          "name": "Market",
          "createdAtUtc": "2024-05-02T11:30:00Z",
          "updatedAtUtc": null
        }
    ]
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `500 Internal Server Error`

---

### 2. Belirli Bir Kategoriyi Getir

Kullanıcıya ait tek bir kategorinin detaylarını ID ile getirir.

*   **Endpoint:** `GET /Categories/{categoryId}`
*   **Açıklama:** Verilen `categoryId`'ye sahip olan ve kullanıcıya ait olan kategorinin detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `CategoryDto` objesi.
    ```json
    {
      "id": 1,
      "name": "Faturalar",
      "createdAtUtc": "2024-05-01T10:00:00Z",
      "updatedAtUtc": "2024-05-20T15:00:00Z"
    }
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `404 Not Found` (Kategori bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`

---

### 3. Yeni Kategori Oluştur

Kullanıcı için yeni bir kategori oluşturur.

*   **Endpoint:** `POST /Categories`
*   **Açıklama:** Gönderilen `name` bilgisine göre yeni bir kategori oluşturur.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`CategoryCreateDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | Kategorinin adı (örn: "Ulaşım"). | Evet |

#### Request Body Örneği
```json
{
  "name": "Eğlence"
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    true
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `500 Internal Server Error`

---

### 4. Kategoriyi Güncelle

Mevcut bir kategorinin adını günceller.

*   **Endpoint:** `PUT /Categories/{categoryId}`
*   **Açıklama:** Belirtilen `categoryId`'ye sahip kategoriyi, gönderilen yeni `name` ile günceller.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`CategoryUpdateDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `name` | `string` | Kategorinin yeni adı. | Evet |

#### Request Body Örneği
```json
{
  "name": "Abonelikler ve Aidatlar"
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** Güncellenmiş `CategoryModel` objesi.
    ```json
    {
        "id": 1,
        "userId": 15,
        "name": "Abonelikler ve Aidatlar",
        "createdAtUtc": "2024-05-01T10:00:00Z",
        "updatedAtUtc": "2024-05-24T18:30:00Z"
    }
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 5. Kategoriyi Sil

Mevcut bir kategoriyi siler.

*   **Endpoint:** `DELETE /Categories/{categoryId}`
*   **Açıklama:** Belirtilen `categoryId`'ye sahip kategoriyi kalıcı olarak siler. Bu işlem geri alınamaz.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `204 No Content`
*   **Açıklama:** Silme işlemi başarılı olduğunda response body'de içerik dönmez.

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`