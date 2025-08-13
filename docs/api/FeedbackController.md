# FinTrack API: Geri Bildirim Yönetimi (Feedback Controller)

Bu doküman, kullanıcıların uygulama hakkında geri bildirim göndermelerini ve kendi gönderdikleri geçmiş geri bildirimleri görüntülemelerini sağlayan `FeedbackController` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/Feedback`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir.

### `type` Alanı Değerleri (`FeedbackType`)

Kullanıcılar geri bildirimlerini sınıflandırabilir. `type` alanı aşağıdaki metin değerlerini alabilir:
*   `BugReport` (Hata Bildirimi)
*   `FeatureRequest` (Özellik İsteği)
*   `GeneralFeedback` (Genel Geri Bildirim)
*   `Question` (Soru)
*   `Other` (Diğer)

---

## Endpoints

### 1. Kullanıcının Tüm Geri Bildirimlerini Getir

Giriş yapmış kullanıcının daha önce gönderdiği tüm geri bildirimleri listeler.

*   **Endpoint:** `GET /Feedback`
*   **Açıklama:** Token'ı gönderen kullanıcıya ait tüm geri bildirimlerin bir listesini döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `FeedbackDto` objelerinden oluşan bir dizi.
    ```json
    [
        {
          "id": 1,
          "subject": "Raporlama Ekranında Hata",
          "description": "Excel formatında rapor oluşturmaya çalıştığımda uygulama donuyor.",
          "type": "BugReport",
          "savedFilePath": "/path/to/screenshot.png",
          "createdAtUtc": "2024-05-23T10:00:00Z",
          "updatedAtUtc": null
        },
        {
          "id": 2,
          "subject": "Kripto Para Desteği",
          "description": "Hesaplarım arasına kripto para cüzdanlarımı da ekleyebilmek harika olurdu.",
          "type": "FeatureRequest",
          "savedFilePath": null,
          "createdAtUtc": "2024-05-24T11:30:00Z",
          "updatedAtUtc": null
        }
    ]
    ```

#### Hata Yanıtları (Error Responses)
*   `404 Not Found`: Kullanıcının hiç geri bildirimi yoksa.
*   `500 Internal Server Error`

---

### 2. Belirli Bir Geri Bildirimi Getir

Kullanıcıya ait tek bir geri bildirimin detaylarını ID ile getirir.

*   **Endpoint:** `GET /Feedback/{Id}`
*   **Açıklama:** Verilen `Id`'ye sahip olan ve kullanıcıya ait olan geri bildirimin detaylarını döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** Tek bir `FeedbackDto` objesi. (Yukarıdaki örnekle aynı yapıdadır.)

#### Hata Yanıtları (Error Responses)
*   `404 Not Found`
*   `500 Internal Server Error`

---

### 3. Yeni Geri Bildirim Oluştur

Kullanıcının yeni bir geri bildirim göndermesini sağlar.

*   **Endpoint:** `POST /Feedback`
*   **Açıklama:** Gönderilen bilgilere göre yeni bir geri bildirim kaydı oluşturur.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`FeedbackCreateDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `subject` | `string` | Geri bildirimin konusu/başlığı. | Evet |
| `description` | `string` | Geri bildirimin detaylı açıklaması. | Evet |
| `type` | `string` | Geri bildirim türü (Bkz. `type` alanı değerleri). | Evet |
| `savedFilePath` | `string` | Varsa, geri bildirimle ilgili bir ekran görüntüsü veya dosyanın sunucudaki yolu. | Hayır |

#### Request Body Örneği
```json
{
  "subject": "Uygulama Performansı",
  "description": "Genel olarak uygulama çok akıcı çalışıyor, teşekkürler!",
  "type": "GeneralFeedback",
  "savedFilePath": null
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    true
    ```

#### Hata Yanıtları (Error Responses)
*   `400 Bad Request`: İstek gövdesi boş veya eksikse.
*   `500 Internal Server Error`