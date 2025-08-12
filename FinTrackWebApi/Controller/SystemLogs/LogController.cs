using Microsoft.AspNetCore.Mvc;

namespace FinTrackWebApi.Controller.SystemLogs
{
    [ApiController]
    [Route("[controller]")]
    public class LogController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LogController> _logger;

        public LogController(IWebHostEnvironment env, ILogger<LogController> logger)
        {
            _env = env;
            _logger = logger;
        }

        [HttpGet("{fileName}")]
        public IActionResult GetLogFile(string fileName)
        {
            try
            {
                var contentRootPath = _env.ContentRootPath;

                var logsDirectoryPath = Path.Combine(contentRootPath, "logs");

                var logFilePath = Path.Combine(logsDirectoryPath, fileName);

                _logger.LogInformation("Request to download log file: {LogFilePath}", logFilePath);

                if (!logFilePath.StartsWith(logsDirectoryPath))
                {
                    _logger.LogWarning("Potential directory traversal attempt blocked for file: {FileName}", fileName);
                    return BadRequest("Invalid file name.");
                }

                if (!System.IO.File.Exists(logFilePath))
                {
                    _logger.LogWarning("Log file not found: {LogFilePath}", logFilePath);
                    return NotFound($"Log file '{fileName}' not found.");
                }

                var fileBytes = System.IO.File.ReadAllBytes(logFilePath);
                string mimeType = "application/json";

                return File(fileBytes, mimeType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving log file for download: {FileName}", fileName);
                return StatusCode(500, "An error occurred while preparing the log file for download.");
            }
        }
    }
}
