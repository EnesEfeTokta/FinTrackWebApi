# FinTrack API: Kullanıcı Ayarları Yönetimi (UserSettings Controller)

Bu doküman, kullanıcıların kendi profil bilgilerini, hesap güvenliklerini, uygulama tercihlerini ve bildirim ayarlarını yönetmelerini sağlayan `UserSettingsController` endpoint'lerini açıklamaktadır.

*Controller Base Path:* `/UserSettings`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir.

---

## 1. Hesap Güvenliği ve Kimlik Yönetimi

### 1.1. Kullanıcı Adını Güncelle

Kullanıcının adını ve soyadını güncelleyerek sistemdeki `UserName`'ini değiştirir.

*   **Endpoint:** `POST /UserSettings/update-username`
*   **Açıklama:** Verilen ad ve soyadı birleştirerek (`Ahmet_Yılmaz` gibi) yeni bir kullanıcı adı oluşturur.

#### Request Body (`UpdateUserNameDto`)
| Alan | Tip | Açıklama |
| :--- | :--- | :--- |
| `firstName` | `string` | Kullanıcının yeni adı. |
| `lastName` | `string` | Kullanıcının yeni soyadı. |

#### Başarılı Yanıt (Success Response)
*   `200 OK` ve `{"message": "Username updated successfully.", "newUserName": "..."}`.

#### Hata Yanıtları (Error Responses)
*   `409 Conflict`: Yeni kullanıcı adı başka bir kullanıcı tarafından alımmışsa.

### 1.2. E-posta Değişikliği Talep Et (Adım 1/2)

Kullanıcının e-posta adresini değiştirmek için **güvenli bir süreç** başlatır.

*   **Endpoint:** `POST /UserSettings/request-email-change`
*   **Açıklama:** Kullanıcının kimliğini doğrulamak için, **mevcut e-posta adresine** 15 dakika geçerli bir OTP kodu gönderir. Bu, hesabın sahibi olmadan e-posta değiştirilmesini engeller.

#### Başarılı Yanıt (Success Response)
*   `200 OK` ve `{"message": "An OTP has been sent to your current email address to verify your identity."}`.

### 1.3. E-posta Değişikliğini Onayla (Adım 2/2)

OTP ile kimliğini doğrulayan kullanıcının e-posta adresini kalıcı olarak değiştirir.

*   **Endpoint:** `POST /UserSettings/confirm-email-change`
*   **Açıklama:** OTP doğrulanırsa, kullanıcının e-postası yeni adrese güncellenir.

#### Request Body (`UpdateUserEmailDto`)
| Alan | Tip | Açıklama |
| :--- | :--- | :--- |
| `newEmail` | `string` | Kullanıcının yeni e-posta adresi. |
| `otpCode` | `string` | Mevcut e-postaya gelen OTP kodu. |

#### Hata Yanıtları (Error Responses)
*   `400 Bad Request`: OTP yanlış veya süresi dolmuşsa.
*   `409 Conflict`: Yeni e-posta adresi başka bir kullanıcı tarafından kullanılıyorsa.

### 1.4. Şifreyi Güncelle

Kullanıcının mevcut şifresini doğrulayarak yeni bir şifre belirlemesini sağlar.

*   **Endpoint:** `POST /UserSettings/update-password`

#### Request Body (`UpdateUserPasswordDto`)
| Alan | Tip | Açıklama |
| :--- | :--- | :--- |
| `currentPassword`| `string` | Kullanıcının mevcut şifresi. |
| `newPassword` | `string` | Kullanıcının yeni şifresi. |

#### Hata Yanıtları (Error Responses)
*   `400 Bad Request`: Mevcut şifre yanlışsa.

---

## 2. Profil ve Uygulama Ayarları

### 2.1. Profil Resmini Güncelle

Kullanıcının profil resminin URL'sini günceller.

*   **Endpoint:** `POST /UserSettings/update-profile-picture`

#### Request Body (`UpdateProfilePictureDto`)
| Alan | Tip | Açıklama |
| :--- | :--- | :--- |
| `profilePictureUrl`| `string` | Yeni profil resminin tam URL'si. |

### 2.2. Uygulama Ayarlarını Getir

Kullanıcının tema, dil ve varsayılan para birimi gibi uygulama genelindeki ayarlarını getirir.

*   **Endpoint:** `GET /UserSettings/app-settings`

### 2.3. Uygulama Ayarlarını Güncelle

Kullanıcının uygulama genelindeki ayarlarını günceller.

*   **Endpoint:** `POST /UserSettings/app-settings`

#### Request Body (`UserAppSettingsUpdateDto`)
| Alan | Tip | Açıklama |
| :--- | :--- | :--- |
| `appearance` | `string` | Tema (`Light`, `Dark`). |
| `currency` | `string` | Varsayılan para birimi (`TRY`, `USD`, `EUR`). |
| `language` | `string` | Uygulama dili (`tr_TR`, `en_US`). |

---

## 3. Bildirim Tercihleri

### 3.1. Bildirim Ayarlarını Getir

Kullanıcının hangi tür bildirimleri almak istediğini belirten ayarları getirir.

*   **Endpoint:** `GET /UserSettings/user-notification-settings`

### 3.2. Bildirim Ayarlarını Güncelle

Kullanıcının bildirim tercihlerini günceller.

*   **Endpoint:** `POST /UserSettings/user-notificationettings` 

#### Request Body (`UserNotificationSettingsUpdateDto`)
| Alan | Tip | Açıklama |
| :--- | :--- | :--- |
| `spendingLimitWarning`| `boolean` | Bütçe limit uyarısı. |
| `expectedBillReminder`| `boolean` | Beklenen fatura hatırlatıcısı. |
| `weeklySpendingSummary`| `boolean`| Haftalık harcama özeti. |
| `newFeaturesAndAnnouncements`| `boolean` | Yeni özellik ve duyurular. |
| `enableDesktopNotifications`| `boolean` | Masaüstü bildirimlerini etkinleştir. |