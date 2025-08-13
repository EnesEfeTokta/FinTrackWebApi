# FinTrack API: GBS Video Yönetimi (Videos Controller)

Bu doküman, Güvenli Borç Sistemi'nin (GBS) temelini oluşturan **video delil mekanizmasını** yöneten `VideosController` endpoint'lerini açıklamaktadır. Bu controller, video yükleme, operatör onayı, şifreleme ve güvenli video akışı (streaming) süreçlerinden sorumludur.

*Controller Base Path:* `/Videos`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. Her endpoint'in kendine özgü rol ve sahiplik bazlı yetkilendirme kuralları vardır.

### Kriptografi ve Güvenlik Modeli

1.  **Geçici Depolama:** Yüklenen videolar ilk olarak sunucuda şifresiz, geçici bir alanda saklanır.
2.  **Operatör Onayı:** Bir operatör videoyu onayladığında, sistem rastgele ve güçlü bir **20 karakterlik şifreleme anahtarı** (`userPasswordKey`) üretir.
3.  **AES Şifreleme:** Video, bu anahtar kullanılarak **AES** algoritması ile şifrelenir ve güvenli bir alana taşınır. Orijinal (şifresiz) dosya kalıcı olarak silinir.
4.  **Anahtar Teslimi:** Üretilen **20 karakterlik anahtar**, borcun alacaklısına (Lender) e-posta ile teslim edilir. Bu anahtar sistemde **saklanmaz**, sadece hash'lenmiş bir versiyonu doğrulama amacıyla tutulur. Anahtarın güvenliği tamamen alacaklının sorumluluğundadır.
5.  **Güvenli Akış (Streaming):** Borç temerrüde düştüğünde, alacaklı elindeki anahtarı kullanarak videoyu deşifre edebilir ve izleyebilir.

### `VideoStatusType` Değerleri
*   `PendingApproval`
*   `ProcessingEncryption`
*   `Encrypted`
*   `Rejected`
*   `ProcessingError`
*   `EncryptionFailed`

---

## Endpoints

### 1. Borçlu Tarafından Video Yükleme (Adım 3)

Borçlunun, kabul ettiği bir borç teklifi için taahhüt videosunu sisteme yüklemesini sağlar.

*   **Endpoint:** `POST /Videos/user-upload-video`
*   **Açıklama:** Bu endpoint `multipart/form-data` formatında bir video dosyası kabul eder. Videoyu geçici olarak sunucuda saklar ve borcun durumunu `PendingOperatorApproval`'a (Operatör Onayı Bekleniyor) günceller.
*   **Yetkilendirme:** Gerekli. Sadece borcun "Borçlusu" (`Borrower`) bu işlemi yapabilir.
*   **İstek Tipi:** `multipart/form-data`

#### Form Verisi
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `file` | `File` | Kullanıcının taahhüt videosu. | Evet |
| `debtId` | `integer`| Videonun ilişkili olduğu borcun ID'si. | Evet |

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** Videonun metadatası.
    ```json
    {
      "message": "Video metadata başarıyla kaydedildi: {VideoMetadata}",
      "videoMetadata": {
        "id": 1,
        "uploadedByUserId": 22,
        "originalFileName": "taahhut.mp4",
        "fileSize": 15728640,
        "contentType": "video/mp4",
        "status": "PendingApproval"
        // ... diğer metadata alanları
      }
    }
    ```

#### Hata Yanıtları (Error Responses)
*   `403 Forbidden`: İşlemi yapan kullanıcı borcun borçlusu değilse.
*   `400 Bad Request`: Borcun durumu video yüklemeye uygun değilse (`AcceptedPendingVideoUpload` değilse).

---

### 2. Videoyu Onaylama ve Şifreleme (Adım 4)

Operatörün, yüklenen videoyu onaylayıp şifreleme sürecini tetiklemesini sağlar.

*   **Endpoint:** `POST /Videos/video-approve/{videoId}`
*   **Açıklama:** Bu endpoint, bir operatör tarafından çağrıldığında videoyu şifreler, orijinal dosyayı siler ve borcun durumunu `Active` (Aktif) yapar. Ardından alacaklıya şifreleme anahtarını içeren bir e-posta gönderir.
*   **Yetkilendirme:** Gerekli. Bu işlemi sadece `VideoApproval` veya `Admin` rolüne sahip kullanıcılar yapabilir.

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
    ```json
    {
      "message": "Video başarıyla onaylandı ve şifrelendi.",
      "videoMeta": {
        "id": 1,
        "status": "Encrypted",
        "storageType": "EncryptedFileSystem"
        // ... diğer metadata alanları
      }
    }
    ```

#### Hata Yanıtları (Error Responses)
*   `404 Not Found`: Video veya ilişkili borç bulunamazsa.
*   `400 Bad Request`: Video zaten işlenmişse.
*   `500 Internal Server Error`: Şifreleme veya e-posta gönderme sırasında bir hata oluşursa.

---

### 3. Şifreli Videoyu İzleme/Akıtma (Adım 6)

Temerrüde düşmüş bir borcun alacaklısının, elindeki anahtar ile videoyu izlemesini sağlar.

*   **Endpoint:** `GET /Videos/video-metadata-stream/{videoId}`
*   **Açıklama:** Bu endpoint, şifrelenmiş video dosyasını, sorgu parametresi (`query parameter`) olarak sağlanan anahtar ile anlık olarak deşifre eder ve istemciye bir dosya akışı (stream) olarak gönderir.
*   **Yetkilendirme:** Gerekli. Sadece borcun "Alacaklısı" (`Lender`) veya `Admin` rolündeki kullanıcılar, borç `Defaulted` (Temerrüde Düştü) durumundayken bu işlemi yapabilir.

#### URL Parametreleri
| Parametre | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `videoId` | `integer`| İzlenmek istenen videonun metadatasının ID'si. | Evet |

#### Sorgu Parametreleri (Query Parameters)
| Parametre | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `key` | `string` | Videoyu deşifre etmek için alacaklıya e-posta ile gönderilen 20 karakterlik anahtar. | Evet |

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content-Type:** Videonun orijinal `Content-Type`'ı (örn: `video/mp4`).
*   **Content:** Deşifre edilmiş video dosyasının kendisi (binary stream).

#### Hata Yanıtları (Error Responses)
*   `401 Unauthorized`: Sağlanan `key` yanlışsa.
*   `403 Forbidden`: Kullanıcı borcun alacaklısı değilse.
*   `400 Bad Request`: Borcun durumu `Defaulted` değilse.