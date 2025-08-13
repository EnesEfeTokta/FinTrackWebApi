# FinTrack API: Sistem Log Yönetimi (Log Controller)

Bu doküman, sunucuda oluşturulan log dosyalarına erişim sağlayarak hata ayıklama ve sistem izleme süreçlerini kolaylaştıran `LogController` endpoint'ini açıklamaktadır.

*Controller Base Path:* `/Log`

---

## Genel Bilgiler

### Yetkilendirme ve Güvenlik Uyarısı

*   **Yetkilendirme:** Bu controller'daki endpoint **halka açıktır** ve herhangi bir yetkilendirme (authentication/authorization) gerektirmez.
*   **DİKKAT:** Bu endpoint'in halka açık olması, üretim (production) ortamlarında ciddi bir güvenlik riski oluşturabilir. Bu nedenle, bu endpoint'e erişim mutlaka bir ağ katmanı güvenliği ile korunmalıdır. Örneğin:
    *   Sadece belirli IP adreslerine izin veren bir **Firewall kuralı** tanımlanmalıdır.
    *   Sadece şirket içi ağdan veya bir **VPN** üzerinden erişilebilir olmalıdır.
    *   Bir **API Gateway** arkasına alınarak erişim kontrolü sağlanmalıdır.

### Dizin Geçişi (Path Traversal) Koruması

Bu endpoint, dosya adlarında `../` gibi ifadeler kullanılarak sistemdeki diğer dosyalara (örn: `appsettings.json`) erişilmesini engellemek için bir **dizin geçişi koruması** içerir. Eğer istenen dosya yolu, beklenen `/logs` dizininin dışına çıkmaya çalışırsa, istek `400 Bad Request` hatası ile reddedilir.

---

## Endpoints

### 1. Belirli Bir Log Dosyasını İndir

Sunucunun `/logs` klasöründe bulunan belirli bir log dosyasını indirir.

*   **Endpoint:** `GET /Log/{fileName}`
*   **Açıklama:** Belirtilen dosya adını kullanarak log dosyasının içeriğini döndürür.
*   **Yetkilendirme:** Gerekmez (`Public`). **Güvenliği ağ katmanında sağlanmalıdır.**

#### URL Parametreleri
| Parametre | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `fileName` | `string` | İndirilmek istenen log dosyasının tam adı ve uzantısı (örn: `fintrack-log-20240525.json`). | Evet |

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content-Type:** `application/json` (Logların JSON formatında olduğu varsayılarak).
*   **Content:** İstenen log dosyasının ham içeriği.

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `400 Bad Request`
    *   `fileName` parametresinde dizin geçişi saldırısı denemesi tespit edilirse (`../` gibi).
*   **Status Code:** `404 Not Found`
    *   Belirtilen `fileName` ile bir log dosyası `/logs` dizininde bulunamazsa.
*   **Status Code:** `500 Internal Server Error`
    *   Dosya okunurken veya indirilirken beklenmedik bir sunucu hatası oluşursa.