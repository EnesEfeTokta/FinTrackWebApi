# FinTrack API: Kullanıcı Profili ve Veri Merkezi (User Controller)

Bu doküman, giriş yapmış kullanıcının tüm kişisel bilgilerini, ayarlarını, üyelik durumunu ve finansal verilerinin özetini tek bir merkezden sağlayan `UserController` endpoint'ini açıklamaktadır.

*Controller Base Path:* `/User`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir.

### Mimarideki Rolü: Veri Toplama Merkezi (Data Aggregator)

Bu controller, istemci uygulamasının (WPF, Mobil vb.) kullanıcı oturumu başladığında ihtiyaç duyabileceği tüm temel verileri **tek bir API çağrısıyla** sunmak için tasarlanmıştır. Bu yaklaşım:
*   İstemcinin başlangıçtaki API çağrı sayısını azaltır.
*   Uygulama açılış performansını artırır.
*   Kullanıcıya ait tüm verilerin tutarlı bir anlık görüntüsünü (snapshot) sağlar.

Controller, arka planda birden fazla veritabanı tablosunu (`Users`, `UserMemberships`, `Accounts`, `Budgets` vb.) birleştirerek zengin bir `UserProfileDto` nesnesi oluşturur.

---

## Endpoints

### 1. Giriş Yapmış Kullanıcının Tüm Bilgilerini Getir

Token sahibi kullanıcının profil bilgilerini, ayarlarını, aktif üyeliğini ve tüm finansal varlıklarının ID listelerini içeren kapsamlı bir veri paketi döndürür.

*   **Endpoint:** `GET /User`
*   **Açıklama:** Bu endpoint, genellikle kullanıcı uygulamaya giriş yaptıktan hemen sonra çağrılır.
*   **Yetkilendirme:** Gerekli (`User` rolü).

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content:** `UserProfileDto` objesi.
    ```json
    {
      // --- Temel Bilgiler ---
      "id": 15,
      "userName": "Ahmet_Yilmaz",
      "email": "ahmet.yilmaz@example.com",
      "profilePictureUrl": "https://.../image.jpg",
      "createdAtUtc": "2024-01-10T14:00:00Z",

      // --- Üyelik Bilgileri ---
      "currentMembershipPlanId": 2,
      "currentMembershipPlanType": "Plus",
      "membershipStartDateUtc": "2024-05-20T10:00:00Z",
      "membershipExpirationDateUtc": "2025-05-20T10:00:00Z",

      // --- Kullanıcı Ayarları ---
      "thema": "Light",
      "language": "tr_TR",
      "currency": "TRY",
      "spendingLimitWarning": true,
      "expectedBillReminder": true,
      "weeklySpendingSummary": false,
      "newFeaturesAndAnnouncements": true,
      "enableDesktopNotifications": true,

      // --- Kullanım Verileri (ID Listeleri) ---
      "currentAccounts": [1, 2, 5],
      "currentBudgets": [10, 11],
      "currentTransactions": [101, 102, 103, 104],
      "currentBudgetsCategories": [20, 21],
      "currentTransactionsCategories": [30, 31, 32],
      "currentLenderDebts": [5],
      "currentBorrowerDebts": [6, 7],
      "currentNotifications": [201, 202],
      "currentFeedbacks": [51],
      "currentVideos": [1]
    }
    ```

#### Hata Yanıtları (Error Responses)
*   `401 Unauthorized`: Geçerli bir token gönderilmediğinde.
*   `500 Internal Server Error`: Veriler toplanırken beklenmedik bir sunucu hatası oluşursa.