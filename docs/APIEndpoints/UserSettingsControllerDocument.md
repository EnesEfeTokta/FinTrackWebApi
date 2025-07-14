# FinTrack API: Kullanıcı Ayarları Yönetimi (UserSettings Controller)

Bu doküman, kullanıcıların tema, dil, para birimi gibi uygulama ayarlarını yönetmek için kullanılan `UserSettings` endpoint'lerini açıklamaktadır.

**Controller Base Path:** `/api/usersettings`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir `Bearer Token` gönderilmelidir. Token'a sahip kullanıcının rolü `User` veya `Admin` olmalıdır.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

Hatalı veya eksik token durumunda `401 Unauthorized` hatası döner.

---

## Endpoints

### 1. Kullanıcının Ayarlarını Getir

Giriş yapmış kullanıcının sistemde kayıtlı ayarlarını getirir.

*   **Endpoint:** `GET /api/usersettings`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait ayarları döndürür. Her kullanıcının sadece bir ayar kaydı bulunur.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** `UserSettingsModel` objesi.
    ```json
    {
      "settingsId": 1,
      "theme": "dark",
      "language": "en",
      "currency": "USD",
      "notification": true,
      "entryDate": "2023-10-20T10:00:00Z",
      "userId": 42
    }
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `404 Not Found`
*   **Açıklama:** Kullanıcıya ait ayar bulunamadığında döner.
*   **Status Code:** `500 Internal Server Error`

---

### 2. Kullanıcı Ayarları Oluştur

Giriş yapmış kullanıcı için yeni ayarlar oluşturur.

*   **Endpoint:** `POST /api/usersettings`
*   **Açıklama:** Kullanıcı için ilk ayar kaydını oluşturur. Mevcut bir ayar varsa bu endpoint'in kullanılması önerilmez.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`UserSettingsDto`)

*   **Content-Type:** `application/json`

| Alan          | Tip      | Açıklama                                       | Zorunlu mu? |
|---------------|----------|------------------------------------------------|-------------|
| `theme`       | `string` | Uygulama teması ("light", "dark" vb.).         | Evet        |
| `language`    | `string` | Uygulama dili ("tr", "en" vb.).                | Evet        |
| `notification`| `boolean`| Bildirimlerin açık olup olmadığı.              | Evet        |
**Not:** `Currency` ve `EntryDate` alanları sunucu tarafından varsayılan değerlerle ayarlanır.

#### Request Body Örneği

```json
{
  "theme": "dark",
  "language": "en",
  "notification": false
}
```

#### Başarılı Cevap (Success Response)

*   **Status Code:** `201 Created`
*   **Headers:**
    *   `Location`: `/api/usersettings`
*   **Content:** Oluşturulan `UserSettingsModel` objesi.
    ```json
    {
      "settingsId": 2,
      "theme": "dark",
      "language": "en",
      "currency": "TRY",
      "notification": false,
      "entryDate": "2023-10-27T14:30:00Z",
      "userId": 42
    }
    ```

#### Hata Cevapları (Error Responses)
*   **Status Code:** `400 Bad Request` (Eksik veya hatalı veri gönderildiğinde).
*   **Status Code:** `500 Internal Server Error`

---

### 3. Kullanıcı Ayarlarını Güncelle

Mevcut kullanıcı ayarlarını günceller.

*   **Endpoint:** `PUT /api/usersettings`
*   **Açıklama:** Giriş yapmış kullanıcının mevcut ayarlarını gönderilen verilerle günceller.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`UserSettingsDto`)
* `POST` metodu ile aynı yapıdadır.

#### Başarılı Cevap (Success Response)

*   **Status Code:** `204 No Content`
*   **Açıklama:** Güncelleme başarılı olduğunda response body'de içerik dönmez.

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Kullanıcının güncellenecek ayarları bulunamazsa).
*   **Status Code:** `500 Internal Server Error`

---

### 4. Kullanıcı Ayarlarını Sil

Kullanıcının ayarlarını siler.

*   **Endpoint:** `DELETE /api/usersettings`
*   **Açıklama:** Giriş yapmış kullanıcının ayarlarını veritabanından kalıcı olarak siler.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Cevap (Success Response)

*   **Status Code:** `204 No Content`
*   **Açıklama:** Silme işlemi başarılı olduğunda response body'de içerik dönmez.

#### Hata Cevapları (Error Responses)
*   **Status Code:** `404 Not Found` (Kullanıcının silinecek ayarları bulunamazsa).
*   **Status Code:** `500 Internal Server Error`