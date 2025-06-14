# FinTrack API: ChatBot Etkileşimi (Chat Controller)

Bu doküman, kullanıcıların yapay zeka destekli ChatBot ile etkileşime geçmesini sağlayan `Chat` endpoint'ini açıklamaktadır. Bu controller, bir aracı (proxy) görevi görerek istekleri güvenli bir şekilde Python ChatBot servisine iletir.

**Controller Base Path:** `/api/chat`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir `Bearer Token` gönderilmelidir. Token'a sahip kullanıcının rolü `User` veya `Admin` olmalıdır.

**Header Örneği:**
`Authorization: Bearer <JWT_TOKENINIZ>`

Hatalı veya eksik token durumunda `401 Unauthorized` hatası döner.

---

## Endpoints

### 1. ChatBot'a Mesaj Gönder

Giriş yapmış kullanıcının mesajını, oturum bilgileriyle birlikte ChatBot servisine iletir ve botun cevabını geri döndürür.

*   **Endpoint:** `POST /api/chat/send`
*   **Açıklama:** Kullanıcıdan gelen mesajı alır, kullanıcının kimlik bilgilerini ve token'ını isteğe ekler ve harici Python ChatBot servisine gönderir. ChatBot'un cevabını doğrudan istemciye geri döndürür.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`ChatRequestDto`)

*   **Content-Type:** `application/json`

| Alan                 | Tip      | Açıklama                                                                                                     | Zorunlu mu? |
|----------------------|----------|--------------------------------------------------------------------------------------------------------------|-------------|
| `message`            | `string` | Kullanıcının ChatBot'a göndermek istediği metin mesajı.                                                        | Evet        |
| `clientChatSessionId`| `string` | Sohbet geçmişini ve bağlamını korumak için istemci tarafından üretilen benzersiz oturum ID'si.                 | Evet        |
| `userId`             | `string` | **Dikkate alınmaz.** Sunucu, bu alanı her zaman isteği yapan kullanıcının token'ından aldığı ID ile doldurur. | Evet (ama boş gönderilebilir) |
| `authToken`          | `string` | **Dikkate alınmaz.** Sunucu, bu alanı her zaman kullanıcının mevcut token'ı ile doldurur.                      | Hayır (boş gönderilebilir) |


#### Request Body Örneği

```json
{
  "message": "Bu ay en çok hangi kategoride harcama yaptım?",
  "clientChatSessionId": "c8a4b21e-7f6a-4b9c-8d1e-2f5a6b7c8d9e",
  "userId": "any_string_or_null",
  "authToken": "any_string_or_null"
}
```

#### Başarılı Cevap (Success Response)

*   **Status Code:** `200 OK`
*   **Content:** ChatBot'tan gelen cevabı içeren `ChatResponseDto` objesi.
    ```json
    {
      "reply": "Bu ayki verilerinize göre en çok harcamayı 'Market Alışverişi' kategorisinde yapmışsınız."
    }
    ```

#### Hata Cevapları (Error Responses)

*   **Status Code:** `400 Bad Request`
*   **Açıklama:** `message` veya `clientChatSessionId` alanları eksik veya geçersiz olduğunda döner.
*   **Status Code:** `401 Unauthorized`
*   **Açıklama:** Geçerli bir `Bearer Token` sağlanmadığında döner.
*   **Status Code:** `500 Internal Server Error`
*   **Açıklama:** API içinde veya Python servisiyle iletişim kurarken beklenmedik bir hata oluştuğunda döner.
*   **Status Code:** `502 Bad Gateway` veya `503 Service Unavailable` (veya diğer 5xx kodları)
*   **Açıklama:** Harici Python ChatBot servisinin kendisi hata verdiğinde veya ulaşılamaz olduğunda, o servisten gelen hata kodu ve mesajı istemciye yansıtılır.