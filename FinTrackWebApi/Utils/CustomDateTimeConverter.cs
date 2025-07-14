using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinTrackWebApi.Utils
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
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
                    _logger.LogWarning("The date string from the API is empty or null.");
                    return default;
                }

                if (
                    DateTime.TryParseExact(
                        dateString,
                        ExpectedApiFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal,
                        out DateTime result
                    )
                )
                {
                    if (result.Kind != DateTimeKind.Utc)
                    {
                        _logger.LogWarning(
                            "TryParseExact AdjustToUniversal is used, but Kind is not Utc: {Kind}",
                            result.Kind
                        );
                        return DateTime.SpecifyKind(result, DateTimeKind.Utc);
                    }
                    return result;
                }
                else
                {
                    _logger.LogError(
                        "DateTime dizesi ayrıştırılamadı: ‘{DateString}’. Beklenen biçim: ‘{ExpectedFormat}’.",
                        dateString,
                        ExpectedApiFormat
                    );
                    throw new JsonException(
                        $"Unable to parse DateTime string '{dateString}'. Expected format: '{ExpectedApiFormat}'."
                    );
                }
            }
            _logger.LogError(
                "Unexpected token type while parsing DateTime: {TokenType}",
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
            writer.WriteStringValue(
                value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture)
            );
        }

        private static readonly ILogger<CustomDateTimeConverter> _logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<CustomDateTimeConverter>();
    }
}
