# FinTrack API: Bildirim Yönetimi (Notification Controller)

Bu doküman, kullanıcılara yönelik uygulama içi bildirimlerin oluşturulması, görüntülenmesi ve yönetilmesi için kullanılan `NotificationController` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/Notification`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir. Tüm işlemler, token sahibi kullanıcı kapsamında gerçekleştirilir.

### `notificationType` Alanı Değerleri (`NotificationType`)

Bildirimler, içeriklerine göre sınıflandırılır. `notificationType` alanı aşağıdaki metin değerlerini alabilir:
*   `Info` (Bilgilendirme)
*   `Warning` (Uyarı)
*   `Error` (Hata)
*   `Success` (Başarı)
*   `System` (Sistem Mesajı)
*   `Debt` (Borç Bildirimi)

---

## Endpoints

### 1. Kullanıcının Tüm Bildirimlerini Getir

Giriş yapmış kullanıcının sistemdeki tüm bildirimlerini, en yeniden en eskiye doğru sıralanmış olarak listeler.

*   **Endpoint:** `GET /Notification`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm bildirimlerin bir listesini döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `NotificationDto` objelerinden oluşan bir dizi.
    ```json
    [
        {
          "id": 1,
          "messageHead": "Yeni Borç Teklifi",
          "messageBody": "Ali Veli adlı kullanıcıdan 500 TL tutarında yeni bir borç teklifi aldınız.",
          "notificationType": "Debt",
          "createdAt": "2024-05-24T10:00:00Z",
          "isRead": false
        },
        {
          "id": 2,
          "messageHead": "Bütçe Uyarısı",
          "messageBody": "'Market' bütçenizin %80'ine ulaştınız.",
          "notificationType": "Warning",
          "createdAt": "2024-05-23T15:30:00Z",
          "isRead": true
        }
    ]
    ```

#### Hata Yanıtları (Error Responses)
*   `500 Internal Server Error`

---

### 2. Tek Bir Bildirimi Okundu Olarak İşaretle

Belirtilen ID'ye sahip tek bir bildirimi "okundu" olarak işaretler.

*   **Endpoint:** `POST /Notification/mark-as-read/{id}`
*   **Açıklama:** Eğer bildirim zaten okunmuşsa, herhangi bir işlem yapmaz.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `204 No Content`
*   **Açıklama:** İşlem başarılı olduğunda response body'de içerik dönmez.

#### Hata Yanıtları (Error Responses)
*   `404 Not Found`: Bildirim bulunamazsa veya kullanıcıya ait değilse.
*   `500 Internal Server Error`

---

### 3. Tüm Bildirimleri Okundu Olarak İşaretle

Kullanıcının okunmamış tüm bildirimlerini tek bir işlemle "okundu" olarak işaretler.

*   **Endpoint:** `POST /Notification/mark-all-as-read`
*   **Açıklama:** Veritabanında toplu bir güncelleme işlemi gerçekleştirir.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `204 No Content`

---

### 4. Yeni Bildirim Oluştur (Genellikle Sistem Tarafından Kullanılır)

Bir kullanıcı için yeni bir bildirim oluşturur. Bu endpoint genellikle doğrudan kullanıcı tarafından değil, sistemdeki diğer olaylar tarafından (örn: yeni bir borç teklifi geldiğinde) tetiklenir.

*   **Endpoint:** `POST /Notification`
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`NotificationCreateDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `messageHead`| `string` | Bildirimin başlığı. | Evet |
| `messageBody`| `string` | Bildirimin detaylı içeriği. | Evet |
|`notificationType`|`string`| Bildirim türü (Bkz. `notificationType` alanları).| Evet |

#### Başarılı Yanıt (Success Response)
*   `201 Created` durum kodu ve oluşturulan bildirimin `NotificationCreateDto` objesi.

---

### 5. Tek Bir Bildirimi Sil

Belirtilen ID'ye sahip tek bir bildirimi kalıcı olarak siler.

*   **Endpoint:** `DELETE /Notification/{id}`
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `204 No Content`

#### Hata Yanıtları (Error Responses)
*   `404 Not Found`: Bildirim bulunamazsa veya kullanıcıya ait değilse.

---

### 6. Tüm Bildirimleri Temizle

Kullanıcının tüm bildirimlerini (okunmuş veya okunmamış) tek bir işlemle kalıcı olarak siler.

*   **Endpoint:** `DELETE /Notification/clear-all`
*   **Açıklama:** Veritabanında toplu bir silme işlemi gerçekleştirir.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `204 No Content`