using System.Diagnostics;

namespace FinTrackWebApi.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await LogRequest(context);

            var originalBodyStream = context.Response.Body;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
                stopwatch.Stop();
                await LogResponse(context, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An unhandled exception was thrown by the downstream middleware. Duration: {Duration}ms", stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task LogRequest(HttpContext context)
        {
            var request = context.Request;
            var requestDetails = new RequestDetails()
            {
                Method = request.Method,
                Scheme = request.Scheme,
                Host = request.Host.Value,
                Path = request.Path.Value,
                QueryString = request.QueryString.Value,
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            var requestLog = new RequestLog
            {
                Type = "[REQUEST] : ",
                Timestamp = DateTime.UtcNow,
                Details = requestDetails
            };

            _logger.LogInformation("{@LogEntry}", requestLog);

            await Task.CompletedTask;
        }

        private async Task LogResponse(HttpContext context, long durationMs)
        {
            var response = context.Response;
            var user = context.User.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "Anonymous";
            var responseDetails = new ResponseDetails
            {
                RequestId = context.TraceIdentifier,
                StatusCode = response.StatusCode,
                User = user,
                DurationMs = durationMs,
                Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            var responseLog = new RequestLog
            {
                Type = "[RESPONSE] : ",
                Timestamp = DateTime.UtcNow,
                Details = responseDetails
            };

            _logger.LogInformation("{@LogEntry}", responseLog);

            await Task.CompletedTask;
        }
    }
}
