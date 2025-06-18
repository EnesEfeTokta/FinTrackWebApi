using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinTrackWebApi.Utils // Namespace'inizi kontrol edin
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        // API bu formatta +00 (UTC) gönderiyor
        private const string ExpectedApiFormat = "yyyy-MM-dd HH:mm:ss+00";

        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? dateString = reader.GetString();
                if (string.IsNullOrWhiteSpace(dateString))
                {
                    // Null veya boş string için varsayılan veya hata
                    // throw new JsonException("Date string is null or empty.");
                    _logger.LogWarning("API'den gelen tarih string'i boş veya null."); // Loglama ekle
                    return default; // Veya uygun bir varsayılan değer
                }

                // ---- ÖNEMLİ DEĞİŞİKLİK BURADA ----
                // DateTimeStyles.AdjustToUniversal, string'deki +00 offset'ini kullanarak
                // sonucu UTC'ye çevirir ve Kind'i Utc yapar.
                if (
                    DateTime.TryParseExact(
                        dateString,
                        ExpectedApiFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal,
                        out DateTime result
                    )
                )
                // ---- /ÖNEMLİ DEĞİŞİKLİK BURADA ----
                {
                    // result.Kind şimdi Utc olmalı
                    if (result.Kind != DateTimeKind.Utc)
                    {
                        // Beklenmedik bir durum, loglayalım
                        _logger.LogWarning(
                            "TryParseExact AdjustToUniversal kullanılmasına rağmen Kind Utc değil: {Kind}",
                            result.Kind
                        );
                        // Güvenlik için tekrar ayarlayalım
                        return DateTime.SpecifyKind(result, DateTimeKind.Utc);
                    }
                    return result;
                }
                else
                {
                    // Loglama ekleyerek hangi formatın geldiğini görebiliriz
                    _logger.LogError(
                        "DateTime string'i parse edilemedi: '{DateString}'. Beklenen format: '{ExpectedFormat}'.",
                        dateString,
                        ExpectedApiFormat
                    );
                    throw new JsonException(
                        $"Unable to parse DateTime string '{dateString}'. Expected format: '{ExpectedApiFormat}'."
                    );
                }
            }
            _logger.LogError(
                "DateTime parse edilirken beklenmedik token tipi: {TokenType}",
                reader.TokenType
            );
            throw new JsonException(
                $"Unexpected token type {reader.TokenType} when parsing DateTime."
            );
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options
        )
        {
            // Her zaman UTC ve ISO 8601 formatında yazmak en güvenlisidir.
            writer.WriteStringValue(
                value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)
            );
        }

        // Loglama için ILogger ekleyin (Dependency Injection ile alınabilir veya statik olabilir)
        // Bu örnekte statik kullanıyoruz, ancak DI daha iyi bir pratiktir.
        private static readonly ILogger<CustomDateTimeConverter> _logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<CustomDateTimeConverter>();
        // Veya Logger'ı dışarıdan alacak bir yapı kurun.
    }
}
