# FinTrack API: FinBot Chat Servisi (Chat Controller)

Bu doküman, kullanıcıların FinTrack'in yapay zeka destekli asistanı **FinBot** ile iletişim kurmasını sağlayan `ChatController` endpoint'ini açıklamaktadır.

*Controller Base Path:* `/Chat`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki endpoint'ler **yetkilendirme gerektirir**. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

### Mimarideki Rolü: Proxy ve Güvenlik Katmanı

`ChatController`, bir **Proxy** (vekil sunucu) görevi görür. Doğrudan yapay zeka servisine (Python FinBotWebApi) erişimi engeller ve bir güvenlik katmanı oluşturur. İş akışı şu şekildedir:

1.  **İstemci (WPF/Mobil) -> ChatController:** Kullanıcı, mesajını bu controller'a gönderir.
2.  **Kimlik Doğrulama:** `ChatController`, gelen `Bearer Token`'ı doğrular ve kullanıcının kimliğini (`userId`) tespit eder.
3.  **İsteği Zenginleştirme:** Gelen isteğe, kullanıcının `userId`'si ve orijinal `authToken`'u gibi önemli bilgileri ekler.
4.  **ChatController -> FinBotWebApi (Python):** Zenginleştirilmiş ve güvenli isteği, arka planda çalışan Python tabanlı yapay zeka servisine iletir.
5.  **Yanıt Akışı:** Python servisinden gelen yanıtı alır ve doğrudan istemciye geri iletir.

Bu yapı, yapay zeka servisinin sadece güvenilir ve kimliği doğrulanmış istekleri işlemesini sağlar.

---

## Endpoints

### 1. FinBot'a Mesaj Gönder

Kullanıcının FinBot ile bir konuşma başlatmasını veya devam ettirmesini sağlar.

*   **Endpoint:** `POST /Chat/send`
*   **Açıklama:** Kullanıcının mesajını ve oturum bilgisini alır, bunu Python'daki FinBot servisine iletir ve oradan gelen yanıtı döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`ChatRequestDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `clientChatSessionId`| `string` | Konuşma oturumunu takip etmek için istemci tarafından üretilen benzersiz bir ID. | Evet |
| `message` | `string` | Kullanıcının FinBot'a gönderdiği metin mesajı. | Evet |

#### Request Body Örneği
```json
{
  "clientChatSessionId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
  "message": "Bu ay market harcamalarım ne kadar oldu?"
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `ChatResponseDto` objesi. Bu yanıt, doğrudan Python servisinden gelir.
    ```json
    {
      "sessionId": "a1b2c3d4-e5f6-7890-1234-567890abcdef",
      "reply": "Bu ayki market harcamalarınız toplam 2,350.50 TL'dir. Geçen aya göre %15'lik bir artış gözlemleniyor. Detaylı bir döküm isterseniz 'Market harcamalarımı listele' diyebilirsiniz.",
      "timestampUtc": "2024-05-24T14:30:00Z"
    }
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `401 Unauthorized`
    *   Geçerli bir token gönderilmediğinde.
*   **Status Code:** `500 Internal Server Error`
    *   `.NET` servisi, Python servisine bağlanamazsa: `{"error": "Failed to contact the ChatBot service."}`
    *   `appsettings.json` dosyasında Python servisinin URL'si eksikse: `{"error": "ChatBot service configuration error."}`
*   **Status Code:** `502 Bad Gateway` veya Python servisinden dönen diğer `5xx` hataları.
    *   Python servisi çökerse veya beklenmedik bir hata döndürürse: `{"error": "ChatBot servisinden beklenmeyen bir yanıt alındı.", "details": "..."}`