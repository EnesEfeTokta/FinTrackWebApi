# FinTrack API: Kullanıcı Kimlik Doğrulama (UserAuth Controller)

Bu doküman, kullanıcıların sisteme kaydolması, kimliklerini doğrulaması ve giriş yapması için kullanılan `UserAuthController` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/UserAuth`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki endpoint'ler halka açıktır (public) ve `Authorization` başlığı gerektirmezler. Kimlik doğrulama işlemleri bu endpoint'ler aracılığıyla gerçekleştirilir.

### İş Akışı: İki Aşamalı Kullanıcı Kaydı

FinTrack, güvenliği sağlamak için iki aşamalı bir kayıt süreci kullanır:

1.  **Aşama 1 (Initiate):** Kullanıcı, temel bilgilerini (`initiate-registration` endpoint'i ile) sisteme gönderir. Sistem, bu bilgileri ve tek kullanımlık bir şifreyi (OTP) geçici olarak veritabanında saklar ve OTP'yi kullanıcının e-posta adresine gönderir.
2.  **Aşama 2 (Verify & Register):** Kullanıcı, e-postasına gelen OTP'yi (`verify-otp-and-register` endpoint'i ile) sisteme gönderir. OTP doğrulanırsa, sistem geçici verileri kullanarak kalıcı kullanıcı kaydını oluşturur, varsayılan ayarları ve üyeliği tanımlar, ardından geçici verileri siler.

Bu yaklaşım, hem e-posta adresinin sahipliğini doğrular hem de geçersiz kayıtların sisteme yük olmasını engeller.

---

## Endpoints

### 1. Kayıt Başlatma ve OTP Gönderimi

Yeni bir kullanıcı kaydının ilk adımını başlatır.

*   **Endpoint:** `POST /UserAuth/initiate-registration`
*   **Açıklama:** Kullanıcıdan alınan bilgileri doğrular, 5 dakika geçerli bir OTP oluşturur, bilgileri geçici olarak saklar ve OTP'yi içeren bir doğrulama e-postası gönderir.
*   **Yetkilendirme:** Gerekmez (Public).

#### Request Body (`UserInitiateRegistrationDto`)

| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `email` | `string` | Kullanıcının geçerli e-posta adresi. | Evet |
| `firstName` | `string` | Kullanıcının adı. | Evet |
| `lastName` | `string` | Kullanıcının soyadı. | Evet |
| `password` | `string` | Kullanıcının belirlediği güçlü bir şifre. | Evet |
| `profilePicture`| `string` | Profil resminin URL'si. | Hayır |

#### Request Body Örneği
```json
{
  "email": "ornek.kullanici@example.com",
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "password": "Password123!",
  "profilePicture": "https://example.com/path/to/image.jpg"
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "message": "OTP has been sent to your email address. Please verify to complete registration."
    }
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `400 Bad Request`
    *   Eksik bilgi gönderildiğinde: `{"message": "Email, Username, and Password are required."}`
    *   E-posta adresi zaten kayıtlı ise: `{"message": "This email address is already registered."}`
    *   Kullanıcı adı (Ad_Soyad) zaten alınmışsa: `{"message": "This username is already taken."}`
*   **Status Code:** `500 Internal Server Error`
    *   OTP veritabanına kaydedilemezse veya e-posta gönderimi sırasında bir hata oluşursa.

---

### 2. OTP Doğrulama ve Kaydı Tamamlama

Kayıt sürecinin ikinci ve son adımını gerçekleştirir.

*   **Endpoint:** `POST /UserAuth/verify-otp-and-register`
*   **Açıklama:** Kullanıcının gönderdiği OTP'yi doğrular. Başarılı ise kalıcı kullanıcı kaydını oluşturur, varsayılan rolleri, ayarları ve üyeliği atar, hoş geldin e-postası gönderir.
*   **Yetkilendirme:** Gerekmez (Public).

#### Request Body (`VerifyOtpRequestDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `email` | `string` | OTP'nin gönderildiği e-posta adresi. | Evet |
| `code` | `string` | E-postaya gelen 6 haneli OTP kodu. | Evet |

#### Request Body Örneği
```json
{
  "email": "ornek.kullanici@example.com",
  "code": "123456"
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "message": "User registration successful. You can now log in.",
      "userId": 123
    }
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `400 Bad Request`
    *   OTP kodu yanlış veya süresi dolmuşsa: `{"message": "Invalid or expired OTP code."}`
    *   ASP.NET Identity şifre politikası gibi bir nedenle kullanıcıyı oluşturamazsa: `{"message": "User registration failed.", "errors": ["Passwords must be at least 6 characters.", "Passwords must have at least one non-alphanumeric character." ... ]}`
*   **Status Code:** `500 Internal Server Error`
    *   Kullanıcı için varsayılan ayarlar oluşturulurken bir hata oluşursa.

---

### 3. Kullanıcı Girişi ve Token Alma

Kayıtlı bir kullanıcının sisteme giriş yapmasını ve oturum token'larını almasını sağlar.

*   **Endpoint:** `POST /UserAuth/login`
*   **Açıklama:** E-posta ve şifre ile kimlik doğrulaması yapar. Başarılı olursa, API'ye erişim için kullanılacak `AccessToken` ve `RefreshToken` üretir.
*   **Yetkilendirme:** Gerekmez (Public).

#### Request Body (`LoginDto`)
| Alan | Tip | Açıklama | Zorunlu mu? |
| :--- | :--- | :--- | :--- |
| `email` | `string` | Kullanıcının kayıtlı e-posta adresi. | Evet |
| `password` | `string` | Kullanıcının şifresi. | Evet |

#### Request Body Örneği
```json
{
  "email": "ornek.kullanici@example.com",
  "password": "Password123!"
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:**
    ```json
    {
      "userId": 123,
      "userName": "Ahmet_Yılmaz",
      "email": "ornek.kullanici@example.com",
      "profilePicture": "https://.../image.jpg",
      "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
      "refreshToken": "another_long_secure_random_string...",
      "roles": ["User"]
    }
    ```

#### Hata Yanıtları (Error Responses)
*   **Status Code:** `401 Unauthorized`
    *   E-posta veya şifre yanlış ise: `{"message": "Invalid credentials."}`
    *   Hesap çok sayıda hatalı deneme nedeniyle kilitlenmişse: `{"message": "Account locked out. Please try again later. (Until: ...)", "isLockedOut": true, "lockoutEndDateUtc": "..."}`