using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinTrackWebApi.Utils
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private const string ExpectedApiFormat = "yyyy-MM-dd HH:mm:ss+00";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? dateString = reader.GetString();

                if (string.IsNullOrWhiteSpace(dateString))
                {
                    throw new JsonException("Date string is null or empty.");
                }

                if (DateTime.TryParseExact(dateString, ExpectedApiFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
                else
                {
                    throw new JsonException($"Unable to parse DateTime string '{dateString}'. Expected format: '{ExpectedApiFormat}'.");
                }
            }

            throw new JsonException($"Unexpected token type {reader.TokenType} when parsing DateTime.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
        }
    }
}