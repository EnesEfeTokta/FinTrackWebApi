using System.Text.Json.Serialization;

namespace FinTrackWebApi.Middlewares
{
    public class RequestLog
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty; // "REQUEST" veya "RESPONSE"

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("details")]
        public object Details { get; set; }
    }

    public class RequestDetails
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("scheme")]
        public string Scheme { get; set; } = string.Empty;

        [JsonPropertyName("host")]
        public string Host { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("queryString")]
        public string QueryString { get; set; } = string.Empty;

        [JsonPropertyName("clientIp")]
        public string? ClientIp { get; set; }

        [JsonPropertyName("headers")]
        public Dictionary<string, string> Headers { get; set; }
    }

    public class ResponseDetails
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;  // Hangi isteğe ait olduğunu belirtmek için

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;

        [JsonPropertyName("durationMs")]
        public long DurationMs { get; set; }

        [JsonPropertyName("headers")]
        public Dictionary<string, string> Headers { get; set; }
    }
}
