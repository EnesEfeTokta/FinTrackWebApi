# FinTrack API: Hesap Yönetimi (Account Controller)

Bu doküman, kullanıcıların finansal hesaplarını (`Account`) yönetmek için kullanılan `AccountController` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/Account`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir. Sistem, token içerisindeki `userId` (NameIdentifier claim) üzerinden kullanıcıyı tanır ve işlemleri sadece o kullanıcı adına gerçekleştirir.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

Hatalı veya eksik token durumunda `401 Unauthorized` hatası döner.

---

## Endpoints

### 1. Kullanıcının Tüm Hesaplarını Getir

Giriş yapmış kullanıcının sistemde kayıtlı tüm hesaplarını listeler.

*   **Endpoint:** `GET /Account`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm hesapların bir listesini döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `AccountDto` objelerinden oluşan bir dizi.
    ```json
    [
        {
          "id": 1,
          "name": "Maaş Hesabı",
          "type": "Bank",
          "isActive": true,
          "balance": 15250.75,
          "currency": "TRY",
          "createdAtUtc": "2024-05-20T10:00:00Z",
          "updatedAtUtc": "2024-05-23T14:30:00Z"
        },
        {
          "id": 2,
          "name": "Nakit Cüzdan",
          "type": "Cash",
          "isActive": true,
          "balance": 850.00,
          "currency": "TRY",
          "createdAtUtc": "2024-05-21T11:00:00Z",
          "updatedAtUtc": "2024-05-22T18:00:00Z"
        }
    ]
    ```

#### Hata Yanıtları (Error Responses)

*   **Status Code:** `500 Internal Server Error`
    ```json
    { "message": "An error occurred while retrieving accounts." }
    ```

---

### 2. Belirli Bir Hesabı Getir

Kullanıcıya ait tek bir hesabın detaylarını ID ile getirir.

*   **Endpoint:** `GET /Account/{Id}`
*   **Açıklama:** Verilen `Id`'ye sahip olan ve token'ı gönderen kullanıcıya ait olan hesabın detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### URL Parametreleri

| Parametre | Tip     | Açıklama                      | Zorunlu mu? |
|-----------|---------|-------------------------------|-------------|
| `Id`      | `integer` | Detayı istenen hesabın ID'si. | Evet        |

#### Başarılı Yanıt (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `AccountDto` objesi.
    ```json
    {
      "id": 1,
      "name": "Maaş Hesabı",
      "type": "Bank",
      "isActive": true,
      "balance": 15250.75,
      "currency": "TRY",
      "createdAtUtc": "2024-05-20T10:00:00Z",
      "updatedAtUtc": "2024-05-23T14:30:00Z"
    }
    ```

#### Hata Yanıtları (Error Responses)

*   **Status Code:** `404 Not Found`
    ```json
    { "message": "Account with ID 101 not found." }
    ```
*   **Status Code:** `500 Internal Server Error`

---

### 3. Yeni Hesap Oluştur

Kullanıcı için yeni bir finansal hesap oluşturur.

*   **Endpoint:** `POST /Account`
*   **Açıklama:** Gönderilen bilgilere göre yeni bir hesap oluşturur ve oluşturulan kaynağı `Location` header'ı ile birlikte döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`AccountCreateDto`)

*   **Content-Type:** `application/json`

| Alan       | Tip       | Açıklama                                 | Zorunlu mu? |
|------------|-----------|------------------------------------------|-------------|
| `name`     | `string`  | Hesabın adı (örn: "Yatırım Hesabım").    | Evet        |
| `type`     | `string`  | Hesap türü. Alabileceği değerler: `Cash`, `Bank`, `CreditCard`, `Investment`, `Other`. | Evet        |
| `isActive` | `boolean` | Hesabın aktif olup olmadığı.             | Evet        |
| `currency` | `string`  | Hesabın para birimi. Alabileceği değerler: `TRY`, `USD`, `EUR` vb. | Evet        |

#### Request Body Örneği

```json
{
  "name": "Euro Hesabı",
  "type": "Bank",
  "isActive": true,
  "currency": "EUR"
}
```

#### Başarılı Yanıt (Success Response)

*   **Status Code:** `201 Created`
*   **Headers:** `Location: /Account/{yeni_hesap_id}`
*   **Content:** Oluşturulan `AccountModel` objesi.
    ```json
    {
        "id": 3,
        "userId": 15,
        "name": "Euro Hesabı",
        "type": "Bank",
        "isActive": true,
        "balance": 0.0,
        "currency": "EUR",
        "createdAtUtc": "2024-05-24T12:00:00Z",
        "updatedAtUtc": null
    }
    ```

#### Hata Yanıtları (Error Responses)

*   **Status Code:** `400 Bad Request`
*   **Status Code:** `500 Internal Server Error`

---

### 4. Hesabı Güncelle

Mevcut bir hesabı günceller.

*   **Endpoint:** `PUT /Account/{Id}`
*   **Açıklama:** Belirtilen `Id`'ye sahip hesabı, gönderilen verilerle günceller.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`AccountUpdateDto`)

*   **Content-Type:** `application/json`

| Alan     | Tip       | Açıklama                                | Zorunlu mu? |
|----------|-----------|-----------------------------------------|-------------|
| `name`   | `string`  | Hesabın yeni adı.                       | Evet        |
| `type`   | `string`  | Hesap türü. Alabileceği değerler: `Cash`, `Bank`, `CreditCard`, `Investment`, `Other`. | Evet        |
| `currency`| `string` | Hesabın yeni para birimi. Alabileceği değerler: `TRY`, `USD`, `EUR` vb. | Evet        |

#### Request Body Örneği
```json
{
  "name": "Dolar Yatırım Hesabı",
  "type": "Investment",
  "currency": "USD"
}
```

#### Başarılı Yanıt (Success Response)

*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    true
    ```

#### Hata Yanıtları (Error Responses)

*   **Status Code:** `400 Bad Request`
*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`

---

### 5. Hesabı Sil

Mevcut bir hesabı siler.

*   **Endpoint:** `DELETE /Account/{Id}`
*   **Açıklama:** Belirtilen `Id`'ye sahip hesabı kalıcı olarak siler. Bu işlem geri alınamaz.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)

*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    true
    ```

#### Hata Yanıtları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Status Code:** `500 Internal Server Error`