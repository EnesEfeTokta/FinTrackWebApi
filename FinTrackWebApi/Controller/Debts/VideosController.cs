using FinTrackWebApi.Data;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.MediaEncryptionService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Debts
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,User,VideoApproval")]
    public class VideosController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<VideosController> _logger;
        private readonly IMediaEncryptionService _mediaEncryptionService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly string _unapprovedVideosPath;
        private readonly string _encryptedVideosPath;

        public VideosController(
            MyDataContext context,
            ILogger<VideosController> logger,
            IMediaEncryptionService mediaEncryptionService,
            IEmailSender emailSender,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment
        )
        {
            _context = context;
            _logger = logger;
            _mediaEncryptionService = mediaEncryptionService;
            _emailSender = emailSender;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;

            _unapprovedVideosPath = _configuration["FilePaths:UnapprovedVideos"] ?? "Null";
            _encryptedVideosPath = _configuration["FilePaths:EncryptedVideos"] ?? "Null";

            if (!Directory.Exists(_unapprovedVideosPath))
                Directory.CreateDirectory(_unapprovedVideosPath);
            if (!Directory.Exists(_encryptedVideosPath))
                Directory.CreateDirectory(_encryptedVideosPath);
        }

        private int GetAuthenticatedId()
        {
            var IdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(IdClaim, out int Id))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }
            return Id;
        }

        [HttpPost("user-upload-video")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UploadVideo(IFormFile file, int debtId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Dosya boş veya geçersiz.");
            }

            int userId = GetAuthenticatedId();

            var videoId = Guid.NewGuid();
            var orginalFileName = Path.GetFileName(file.FileName);
            var storedFileName = $"{videoId}_{orginalFileName}";
            var unencryptedFilePath = Path.Combine(_unapprovedVideosPath, storedFileName);

            try
            {
                using (var stream = new FileStream(unencryptedFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var videoMetadata = new VideoMetadataModel
                {
                    UploadedByUserId = userId,
                    OriginalFileName = orginalFileName,
                    StoredFileName = storedFileName,
                    UnencryptedFilePath = unencryptedFilePath,
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    UploadDateUtc = DateTime.UtcNow,
                    Status = VideoStatusType.PendingApproval,
                    EncryptionKeyHash = _mediaEncryptionService.HashKey(
                        _mediaEncryptionService.GenerateRandomKey(20)
                    ),
                    EncryptionSalt = _mediaEncryptionService.GenerateSalt(),
                    EncryptionIV = _mediaEncryptionService.GenerateIV(),
                };

                await _context.VideoMetadatas.AddAsync(videoMetadata);
                await _context.SaveChangesAsync();

                var debtVideoMetadata = new DebtVideoMetadataModel
                {
                    DebtId = debtId,
                    VideoMetadataId = videoMetadata.Id,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = DateTime.UtcNow,
                    Status = VideoStatusType.PendingApproval,
                };

                await _context.DebtVideoMetadatas.AddAsync(debtVideoMetadata);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Video metadata başarıyla kaydedildi: {VideoMetadata}",
                    videoMetadata
                );
                return Ok(
                    new
                    {
                        Message = "Video metadata başarıyla kaydedildi: {VideoMetadata}",
                        videoMetadata,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya kaydedilirken hata oluştu.");
                return StatusCode(500, "Dosya kaydedilirken hata oluştu.");
            }
        }

        [HttpPost("video-approve/{videoId}")]
        [Authorize]
        public async Task<IActionResult> ApproveAndEncryptVideo([FromRoute] int videoId)
        {
            var videoMetadata = await _context
                .DebtVideoMetadatas.Include(v => v.Debt)
                .ThenInclude(vu => vu.Lender)
                .Include(v => v.Debt)
                .ThenInclude(vu => vu.Borrower)
                .Include(v => v.VideoMetadata)
                .ThenInclude(v => v.UploadedUser)
                .Include(dvm => dvm.Debt)
                .ThenInclude(d => d.Currency)
                .FirstOrDefaultAsync(v => v.VideoMetadataId == videoId);

            if (videoMetadata == null)
            {
                return NotFound("Video metadata bulunamadı.");
            }
            if (videoMetadata.Status != VideoStatusType.PendingApproval)
            {
                return BadRequest("Video zaten onaylanmış veya reddedilmiş.");
            }

            try
            {
                if (
                    string.IsNullOrEmpty(videoMetadata.VideoMetadata?.UnencryptedFilePath)
                    || !System.IO.File.Exists(videoMetadata.VideoMetadata?.UnencryptedFilePath)
                )
                {
                    videoMetadata.Status = VideoStatusType.ProcessingError;
                    await _context.SaveChangesAsync();
                    return NotFound("Şifrelenmemiş video dosyası bulunamadı.");
                }

                videoMetadata.Status = VideoStatusType.ProcessingEncryption;
                await _context.SaveChangesAsync();

                string userPasswordKey = _mediaEncryptionService.GenerateRandomKey(20);
                string salt = _mediaEncryptionService.GenerateSalt();
                string iv = _mediaEncryptionService.GenerateIV();

                var encryptedFileName = $"enc_{videoMetadata.VideoMetadata.StoredFileName}";
                var encryptedFilePath = Path.Combine(_encryptedVideosPath, encryptedFileName);

                await _mediaEncryptionService.EncryptFileAsync(
                    videoMetadata.VideoMetadata.UnencryptedFilePath,
                    encryptedFilePath,
                    userPasswordKey,
                    salt,
                    iv
                );

                videoMetadata.VideoMetadata.EncryptedFilePath = encryptedFilePath;
                videoMetadata.VideoMetadata.EncryptionKeyHash = _mediaEncryptionService.HashKey(
                    userPasswordKey
                );
                videoMetadata.VideoMetadata.EncryptionSalt = salt;
                videoMetadata.VideoMetadata.EncryptionIV = iv;
                videoMetadata.VideoMetadata.Status = VideoStatusType.Encrypted;
                videoMetadata.VideoMetadata.StorageType = VideoStorageType.EncryptedFileSystem;

                System.IO.File.Delete(videoMetadata.VideoMetadata.UnencryptedFilePath);
                videoMetadata.VideoMetadata.UnencryptedFilePath = "This file is now encrypted.";

                await _context.SaveChangesAsync();

                try
                {
                    string emailSubject = "Debt Video Approved and Encrypted";
                    string emailBody = string.Empty;

                    string emailTemplatePath = Path.Combine(
                        _webHostEnvironment.ContentRootPath,
                        "Services",
                        "EmailService",
                        "EmailHtmlSchemes",
                        "OperatorDebtApprovalScheme.html"
                    );
                    if (!System.IO.File.Exists(emailTemplatePath))
                    {
                        _logger.LogError("Email template not found at {Path}", emailTemplatePath);
                        return StatusCode(500, "Email template not found.");
                    }

                    using (StreamReader reader = new StreamReader(emailTemplatePath))
                    {
                        emailBody = await reader.ReadToEndAsync();
                    }

                    emailBody = emailBody.Replace(
                        "[VIDEO_FILE_NAME]",
                        videoMetadata.VideoMetadata.OriginalFileName
                    );
                    emailBody = emailBody.Replace(
                        "[VIDEO_FILE_SIZE]",
                        videoMetadata.VideoMetadata.FileSize.ToString() ?? "N/A"
                    );
                    emailBody = emailBody.Replace(
                        "[USER_NAME]",
                        videoMetadata.VideoMetadata.UploadedUser?.UserName
                    );
                    emailBody = emailBody.Replace(
                        "[LENDER_NAME]",
                        videoMetadata.Debt?.Lender?.UserName
                    );
                    emailBody = emailBody.Replace(
                        "[DETAIL_LENDER_NAME]",
                        videoMetadata.Debt?.Lender?.UserName ?? "N/A"
                    );
                    emailBody = emailBody.Replace(
                        "[DETAIL_BORROWER_NAME]",
                        videoMetadata.Debt?.Borrower?.UserName ?? "N/A"
                    );
                    emailBody = emailBody.Replace(
                        "[DETAIL_DEBT_AMOUNT]",
                        videoMetadata.Debt?.Amount.ToString()
                    );
                    emailBody = emailBody.Replace(
                        "[DETAIL_DEBT_CURRENCY]",
                        videoMetadata?.Debt?.Currency.ToString()
                    );
                    emailBody = emailBody.Replace(
                        "[DETAIL_DEBT_DUE_DATE]",
                        videoMetadata?.Debt?.DueDateUtc.ToString() ?? "N/A"
                    );
                    emailBody = emailBody.Replace(
                        "[DETAIL_DEBT_DESCRIPTION]",
                        videoMetadata?.Debt?.Description
                    );
                    emailBody = emailBody.Replace("[APPROVAL_DATE]", DateTime.UtcNow.ToString());
                    emailBody = emailBody.Replace(
                        "[AGREEMENT_ID]",
                        videoMetadata?.Debt?.Id.ToString()
                    );

                    emailBody = emailBody.Replace(
                        "[VIDEO_FILE_NAME]",
                        videoMetadata?.VideoMetadata.OriginalFileName ?? "N/A"
                    );
                    emailBody = emailBody.Replace("[ENCRYPTION_KEY]", userPasswordKey ?? "N/A");

                    emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

                    await _emailSender.SendEmailAsync(
                        videoMetadata?.Debt?.Lender?.Email ?? "N/A",
                        emailSubject,
                        emailBody
                    );

                    _logger.LogInformation(
                        "Onaylanmış ve şifrelenmiş video için e-posta gönderildi: {Email}",
                        videoMetadata?.VideoMetadata.UploadedUser?.Email
                    );
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "E-posta gönderilirken hata oluştu.");
                    return StatusCode(500, "E-posta gönderilirken hata oluştu.");
                }

                _logger.LogInformation(
                    "Video başarıyla onaylandı ve şifrelendi: {VideoMetadata}",
                    videoMetadata
                );
                return Ok(
                    new { Message = "Video başarıyla onaylandı ve şifrelendi.", videoMetadata }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video şifrelenirken hata oluştu.");
                videoMetadata.Status = VideoStatusType.ProcessingError;
                await _context.SaveChangesAsync();
                return StatusCode(500, "Video şifrelenirken hata oluştu.");
            }
        }

        [HttpGet("video-metadata-stream/{videoId}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> StreamVideo(
            [FromRoute] int videoId,
            [FromQuery] string key
        )
        {
            if (string.IsNullOrEmpty(key))
                return BadRequest("Şifreleme anahtarı (key) gereklidir.");

            var video = await _context.VideoMetadatas.FindAsync(videoId);
            if (video == null)
                return NotFound("Video bulunamadı.");

            if (
                video.Status != VideoStatusType.Encrypted
                || string.IsNullOrEmpty(video.EncryptedFilePath)
                || string.IsNullOrEmpty(video.EncryptionKeyHash)
                || string.IsNullOrEmpty(video.EncryptionSalt)
                || string.IsNullOrEmpty(video.EncryptionIV)
            )
            {
                return BadRequest(
                    "Video izlenmeye uygun değil veya gerekli şifreleme bilgileri eksik."
                );
            }

            var providedKeyHash = _mediaEncryptionService.HashKey(key);
            if (providedKeyHash != video.EncryptionKeyHash)
            {
                return Unauthorized("Geçersiz şifreleme anahtarı.");
            }

            try
            {
                var decryptedStream = _mediaEncryptionService.GetDecryptedVideoStream(
                    video.EncryptedFilePath,
                    key,
                    video.EncryptionSalt,
                    video.EncryptionIV
                );
                if (decryptedStream == null)
                {
                    return NotFound("Şifrelenmiş video dosyası bulunamadı veya şifre çözülemedi.");
                }
                return File(
                    decryptedStream,
                    video.ContentType,
                    video.OriginalFileName ?? "video.mp4"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video akışı sırasında hata oluştu.");
                return StatusCode(500, "Video akışı sırasında hata oluştu.");
            }
        }
    }
}
