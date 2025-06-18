# FinTrack API: Hesap Yönetimi (Account Controller)

Bu doküman, kullanıcıların finansal hesaplarını yönetmek için kullanılan `Account` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/api/account`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir `Bearer Token` gönderilmelidir. Token'a sahip kullanıcının rolü `User` veya `Admin` olmalıdır.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

Hatalı veya eksik token durumunda `401 Unauthorized` hatası döner.

---

## Endpoints

### 1. Kullanıcının Tüm Hesaplarını Getir

Giriş yapmış kullanıcının sistemde kayıtlı tüm hesaplarını listeler.

*   **Endpoint:** `GET /api/account`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm hesapların bir listesini, hesaplanan güncel bakiyeleri ile birlikte döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `AccountDto` objelerinden oluşan bir dizi.
    ```json
    [
        {
          "accountId": 0,
          "userId": 0,
          "name": "string",
          "type": 0,
          "isActive": true,
          "balance": 0,
          "createdAtUtc": "2025-06-13T21:18:01.824Z",
          "updatedAtUtc": "2025-06-13T21:18:01.824Z"
        }
    ]
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `500 Internal Server Error`
*   **Açıklama:** Sunucu tarafında beklenmedik bir hata oluştuğunda döner.
    ```json
    {
      "message": "An error occurred while retrieving accounts."
    }
    ```

---

### 2. Belirli Bir Hesabı Getir

Kullanıcıya ait tek bir hesabın detaylarını ID ile getirir.

*   **Endpoint:** `GET /api/account/{accountId}`
*   **Açıklama:** Verilen `accountId`'ye sahip olan ve token'ı gönderen kullanıcıya ait olan hesabın detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri

| Parametre    | Tip     | Açıklama                      | Zorunlu mu? |
|--------------|---------|-------------------------------|-------------|
| `accountId`  | `integer` | Detayı istenen hesabın ID'si. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `AccountDto` objesi.
    ```json
    {
      "accountId": 0,
      "userId": 0,
      "name": "string",
      "type": 0,
      "isActive": true,
      "balance": 0,
      "createdAtUtc": "2025-06-13T21:18:01.824Z",
      "updatedAtUtc": "2025-06-13T21:18:01.824Z"
    }
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Açıklama:** Belirtilen `accountId` ile bir hesap bulunamadığında veya bulunan hesap kullanıcıya ait olmadığında döner.
    ```json
    {
      "message": "Account with ID 101 not found."
    }
    ```
*   **Status Code:** `500 Internal Server Error`

---

### 3. Yeni Hesap Oluştur

Kullanıcı için yeni bir finansal hesap oluşturur.

*   **Endpoint:** `POST /api/account`
*   **Açıklama:** Gönderilen bilgilere göre yeni bir hesap oluşturur ve oluşturulan kaynağı döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body

*   **Content-Type:** `application/json`

| Alan      | Tip      | Açıklama                                 | Zorunlu mu? |
|-----------|----------|------------------------------------------|-------------|
| `name`    | `string` | Hesabın adı (örn: "Nakit Cüzdan").       | Evet        |
| `type`    | `string` | Hesabın türü (örn: "Nakit", "Banka").    | Evet        |
| `balance` | `number` | Hesabın başlangıç bakiyesi.              | Evet        |

#### Request Body Örneği

```json
{
  "userId": 0,
  "name": "string",
  "type": 0,
  "isActive": true,
  "balance": 0.01,
  "createdAtUtc": "2025-06-13T21:25:42.698Z",
  "updatedAtUtc": "2025-06-13T21:25:42.698Z"
}
```

#### Başarılı Cevap (Success Response)

*   **Status Code:** `201 Created`
*   **Headers:**
    *   `Location`: `/api/account/{yeni_hesap_id}` (Oluşturulan kaynağın URL'si)
*   **Content:** Oluşturulan `AccountModel` objesi.
    ```json
    {
        "userId": 0,
        "name": "string",
        "type": 0,
        "balance": 0.01,
        "isActive": true,
        "createdAtUtc": "2023-10-27T12:00:00Z",
        "updatedAtUtc": "2023-10-27T12:00:00Z"
    }
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `400 Bad Request`
*   **Açıklama:** Request body boş veya hatalı gönderildiğinde.
*   **Status Code:** `500 Internal Server Error`

---

### 4. Hesabı Güncelle

Mevcut bir hesabı günceller.

*   **Endpoint:** `PUT /api/account/{accountId}`
*   **Açıklama:** Belirtilen `accountId`'ye sahip hesabı, gönderilen verilerle günceller.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri

| Parametre    | Tip     | Açıklama                        | Zorunlu mu? |
|--------------|---------|---------------------------------|-------------|
| `accountId`  | `integer` | Güncellenecek hesabın ID'si.    | Evet        |

#### Request Body

*   **Content-Type:** `application/json`

| Alan      | Tip      | Açıklama                       | Zorunlu mu? |
|-----------|----------|--------------------------------|-------------|
| `name`    | `string` | Hesabın yeni adı.              | Evet        |
| `type`    | `string` | Hesabın yeni türü.             | Evet        |
| `balance` | `number` | Hesabın yeni güncel bakiyesi.  | Evet        |

#### Request Body Örneği

```json
{
  "name": "String",
  "type": 0,
  "balance": 0.01
}
```

#### Başarılı Cevap (Success Response)

*   **Status Code:** `204 No Content`
*   **Açıklama:** Güncelleme başarılı olduğunda response body'de içerik dönmez.

#### Hata Cevapları (Error Responses)

*   **Status Code:** `400 Bad Request`
*   **Status Code:** `404 Not Found` (Hesap bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`

---

### 5. Hesabı Sil

Mevcut bir hesabı siler.

*   **Endpoint:** `DELETE /api/account/{accountId}`
*   **Açıklama:** Belirtilen `accountId`'ye sahip hesabı kalıcı olarak siler. Bu işlem geri alınamaz.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri

| Parametre    | Tip     | Açıklama                 | Zorunlu mu? |
|--------------|---------|--------------------------|-------------|
| `accountId`  | `integer` | Silinecek hesabın ID'si. | Evet        |

#### Başarılı Cevap (Success Response)

*   **Status Code:** `204 No Content`
*   **Açıklama:** Silme işlemi başarılı olduğunda response body'de içerik dönmez.

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found` (Hesap bulunamazsa veya kullanıcıya ait değilse).
*   **Status Code:** `500 Internal Server Error`