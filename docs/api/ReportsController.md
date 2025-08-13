# FinTrack API: Raporlama Servisi (Reports Controller)

Bu doküman, kullanıcıların finansal verilerini (işlemler, hesaplar, bütçeler) analiz edip, bu verileri çeşitli formatlarda profesyonel raporlara dönüştürmesini sağlayan `ReportsController` endpoint'ini açıklamaktadır.

*Controller Base Path:* `/Reports`

---

## Genel Bilgiler

### Yetkilendirme (Authentication)

Bu controller'daki **tüm endpoint'ler** yetkilendirme gerektirir. İsteklerin `Authorization` başlığında geçerli bir JWT `Bearer Token` gönderilmelidir.

### Mimarideki Rolü: Veri Toplama ve Döküman Üretimi

`ReportsController`, bir orkestratör görevi görür:

1.  **Tek Bir Giriş Noktası:** Tüm raporlama istekleri tek bir endpoint olan `/generate` üzerinden alınır.
2.  **Dinamik Veri Toplama:** Gelen isteğin `reportType` alanına göre (`Transaction`, `Account`, `Budget`), ilgili verileri veritabanından toplamak için özel bir metot çağrılır. Bu metotlar, isteğin içindeki diğer filtreleri (tarih aralığı, hesap ID'leri vb.) kullanarak veritabanı sorgusunu dinamik olarak oluşturur.
3.  **Döküman Servisine Devretme:** Toplanan ve formatlanan veriler (`IReportModel`), yine istekte belirtilen `exportFormat` (`PDF`, `Excel` vb.) bilgisi ile birlikte `IDocumentGenerationService`'e gönderilir.
4.  **Dosya Yanıtı:** Döküman servisi tarafından üretilen rapor (PDF, Excel dosyası vb.), `File` yanıtı olarak doğrudan kullanıcıya indirilebilir şekilde döndürülür.

### `ReportType` ve `ExportFormat` Değerleri

*   **`reportType`:** Hangi türde rapor oluşturulacağını belirtir.
    *   `Transaction`
    *   `Account`
    *   `Budget`
*   **`exportFormat`:** Raporun hangi formatta çıktılanacağını belirtir.
    *   `PDF`
    *   `Word`
    *   `Excel`
    *   `XML`
    *   `Text`
    *   `Markdown`

---

## Endpoints

### 1. Dinamik Rapor Oluştur

Kullanıcının belirlediği kriterlere göre finansal bir rapor oluşturur ve belirtilen formatta dosyayı döndürür.

*   **Endpoint:** `POST /Reports/generate`
*   **Açıklama:** Bu çok amaçlı endpoint, gönderilen `ReportRequestDto` içindeki verilere göre farklı raporlar üretebilir.
*   **Yetkilendirme:** Gerekli (`User` veya `Admin` rolü).

#### Request Body (`ReportRequestDto`)

Bu DTO, hangi raporun istenildiğine göre farklı alanları kullanır.

| Alan | Tip | Açıklama | Hangi Raporlar İçin Geçerli? |
| :--- | :--- | :--- | :--- |
| `reportType`| `string`| Oluşturulacak raporun türü. | **Tümü (Zorunlu)** |
| `exportFormat`| `string`| Çıktı dosyasının formatı. | **Tümü (Zorunlu)** |
| `startDate` | `string`| Raporun başlangıç tarihi (ISO 8601). | `Transaction`, `Budget` |
| `endDate` | `string` | Raporun bitiş tarihi (ISO 8601). | `Transaction`, `Budget` |
| `date` | `string` | Sadece tek bir günün işlemlerini almak için (ISO 8601).| `Transaction` |
| `selectedAccountIds`|`integer[]`| Sadece belirtilen hesap ID'lerine ait verileri dahil eder.| `Transaction`, `Account` |
| `selectedCategoryIds`|`integer[]`| Sadece belirtilen kategori ID'lerine ait verileri dahil eder.| `Transaction`, `Budget` |
| `selectedBudgetIds`|`integer[]`| Sadece belirtilen bütçe ID'lerine ait verileri dahil eder.| `Budget` |
| `isIncomeSelected`|`boolean` | Gelir işlemlerini dahil et (`true`/`false`).| `Transaction` |
| `isExpenseSelected`|`boolean` | Gider işlemlerini dahil et (`true`/`false`).| `Transaction` |
| `minBalance`| `number` | Minimum hesap bakiyesi filtresi. | `Account` |
| `maxBalance` | `number` | Maksimum hesap bakiyesi filtresi. | `Account` |

#### Request Body Örneği (İşlem Raporu)
```json
{
  "reportType": "Transaction",
  "exportFormat": "PDF",
  "startDate": "2024-05-01T00:00:00Z",
  "endDate": "2024-05-31T23:59:59Z",
  "selectedAccountIds": [1, 3],
  "isExpenseSelected": true,
  "isIncomeSelected": false
}
```

#### Başarılı Yanıt (Success Response)
*   **Status Code:** `200 OK`
*   **Content-Type:** İstenen formata uygun MIME türü (örn: `application/pdf`, `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`).
*   **Content:** Oluşturulan rapor dosyasının kendisi (binary).

#### Hata Yanıtları (Error Responses)
*   `400 Bad Request`: `reportType` veya `exportFormat` desteklenmiyorsa veya istek geçersizse.
*   `404 Not Found`: Belirtilen kriterlere uygun hiçbir veri bulunamazsa.
*   `500 Internal Server Error`: Rapor oluşturma sırasında beklenmedik bir hata oluşursa.