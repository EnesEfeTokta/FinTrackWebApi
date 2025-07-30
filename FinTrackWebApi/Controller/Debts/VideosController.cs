using FinTrackWebApi.Data;
using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.Debt;
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

        // Video yükleme endpointi
        [HttpPost("user-upload-video")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UploadVideo(IFormFile file, int debtId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Video file is required.");
            }

            int userId = GetAuthenticatedId();

            var debt = await _context.Debts.FindAsync(debtId);
            if (debt == null)
            {
                return NotFound("Debt not found.");
            }

            if (debt.BorrowerId != userId)
            {
                return Forbid("You are not the borrower for this debt and cannot upload a video.");
            }

            if (debt.Status != DebtStatusType.AcceptedPendingVideoUpload)
            {
                return BadRequest($"You can only upload a video for debts pending video upload. Current status: {debt.Status}");
            }

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

                debt.Status = DebtStatusType.PendingOperatorApproval;
                debt.UpdatedAtUtc = DateTime.UtcNow;
                _context.Debts.Update(debt);

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

        // Video onaylama ve şifreleme endpointi
        [HttpPost("video-approve/{videoId}")]
        [Authorize(Roles = "Admin,User")] // TODO: Test için User rolü ile erişim verildi, gerçek senaryoda sadece VideoApproval rolü olacak.
        public async Task<IActionResult> ApproveAndEncryptVideo([FromRoute] int videoId)
        {
            var debtVideoMetadata = await _context
                .DebtVideoMetadatas
                .Include(dvm => dvm.Debt).ThenInclude(d => d.Lender)
                .Include(dvm => dvm.Debt).ThenInclude(d => d.Borrower)
                .Include(dvm => dvm.VideoMetadata).ThenInclude(vm => vm.UploadedUser)
                .FirstOrDefaultAsync(dvm => dvm.VideoMetadataId == videoId);

            if (debtVideoMetadata?.VideoMetadata == null || debtVideoMetadata.Debt == null)
            {
                return NotFound("Video metadata or associated debt not found.");
            }
            if (debtVideoMetadata.Status != VideoStatusType.PendingApproval)
            {
                return BadRequest("This video has already been processed (approved or rejected).");
            }

            var videoMeta = debtVideoMetadata.VideoMetadata;
            var debt = debtVideoMetadata.Debt;

            try
            {
                if (string.IsNullOrEmpty(videoMeta.UnencryptedFilePath) || !System.IO.File.Exists(videoMeta.UnencryptedFilePath))
                {
                    videoMeta.Status = VideoStatusType.ProcessingError;
                    await _context.SaveChangesAsync();
                    return NotFound("Unencrypted video file not found.");
                }

                debtVideoMetadata.Status = VideoStatusType.ProcessingEncryption;
                videoMeta.Status = VideoStatusType.ProcessingEncryption;
                await _context.SaveChangesAsync();

                string userPasswordKey = _mediaEncryptionService.GenerateRandomKey(20);
                string salt = _mediaEncryptionService.GenerateSalt();
                string iv = _mediaEncryptionService.GenerateIV();

                var encryptedFileName = $"enc_{videoMeta.StoredFileName}";
                var encryptedFilePath = Path.Combine(_encryptedVideosPath, encryptedFileName);

                await _mediaEncryptionService.EncryptFileAsync(
                    videoMeta.UnencryptedFilePath,
                    encryptedFilePath,
                    userPasswordKey,
                    salt,
                    iv
                );

                videoMeta.EncryptedFilePath = encryptedFilePath;
                videoMeta.EncryptionKeyHash = _mediaEncryptionService.HashKey(userPasswordKey);
                videoMeta.EncryptionSalt = salt;
                videoMeta.EncryptionIV = iv;
                videoMeta.Status = VideoStatusType.Encrypted;
                videoMeta.StorageType = VideoStorageType.EncryptedFileSystem;

                System.IO.File.Delete(videoMeta.UnencryptedFilePath);
                videoMeta.UnencryptedFilePath = "This file is now encrypted.";

                debt.Status = DebtStatusType.Active;
                debt.OperatorApprovalAtUtc = DateTime.UtcNow;
                debt.UpdatedAtUtc = DateTime.UtcNow;

                debtVideoMetadata.Status = VideoStatusType.Encrypted;
                debtVideoMetadata.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                try
                {
                    await SendApprovalEmail(debt, videoMeta, userPasswordKey);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Video was encrypted, but failed to send the email notification for debt ID {DebtId}.", debt.Id);
                    return StatusCode(500, "Video encrypted, but email notification failed.");
                }

                _logger.LogInformation(
                    "Video başarıyla onaylandı ve şifrelendi: {VideoMetadata}",
                    videoMeta
                );
                return Ok(
                    new { Message = "Video başarıyla onaylandı ve şifrelendi.", videoMeta }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video şifrelenirken hata oluştu.");
                debtVideoMetadata.Status = VideoStatusType.EncryptionFailed;
                videoMeta.Status = VideoStatusType.EncryptionFailed;
                await _context.SaveChangesAsync();
                return StatusCode(500, "Video şifrelenirken hata oluştu.");
            }
        }

        private async Task SendApprovalEmail(DebtModel debt, VideoMetadataModel videoMeta, string encryptionKey)
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
                return;
            }

            using (StreamReader reader = new StreamReader(emailTemplatePath))
            {
                emailBody = await reader.ReadToEndAsync();
            }

            emailBody = emailBody.Replace(
                "[VIDEO_FILE_NAME]",
                videoMeta.OriginalFileName
            );
            emailBody = emailBody.Replace(
                "[VIDEO_FILE_SIZE]",
                videoMeta.FileSize.ToString() ?? "N/A"
            );
            emailBody = emailBody.Replace(
                "[USER_NAME]",
                videoMeta.UploadedUser?.UserName
            );
            emailBody = emailBody.Replace(
                "[LENDER_NAME]",
                debt?.Lender?.UserName
            );
            emailBody = emailBody.Replace(
                "[DETAIL_LENDER_NAME]",
                debt?.Lender?.UserName ?? "N/A"
            );
            emailBody = emailBody.Replace(
                "[DETAIL_BORROWER_NAME]",
                debt?.Borrower?.UserName ?? "N/A"
            );
            emailBody = emailBody.Replace(
                "[DETAIL_DEBT_AMOUNT]",
                debt?.Amount.ToString()
            );
            emailBody = emailBody.Replace(
                "[DETAIL_DEBT_CURRENCY]",
                debt?.Currency.ToString()
            );
            emailBody = emailBody.Replace(
                "[DETAIL_DEBT_DUE_DATE]",
                debt?.DueDateUtc.ToString() ?? "N/A"
            );
            emailBody = emailBody.Replace(
                "[DETAIL_DEBT_DESCRIPTION]",
                debt?.Description
            );
            emailBody = emailBody.Replace("[APPROVAL_DATE]", DateTime.UtcNow.ToString());
            emailBody = emailBody.Replace(
                "[AGREEMENT_ID]",
                debt?.Id.ToString()
            );

            emailBody = emailBody.Replace(
                "[VIDEO_FILE_NAME]",
                videoMeta?.OriginalFileName ?? "N/A"
            );
            emailBody = emailBody.Replace("[ENCRYPTION_KEY]", encryptionKey ?? "N/A");

            emailBody = emailBody.Replace("[YEAR]", DateTime.UtcNow.ToString("yyyy"));

            await _emailSender.SendEmailAsync(
                debt?.Lender?.Email ?? "N/A",
                emailSubject,
                emailBody
            );

            _logger.LogInformation(
                "Onaylanmış ve şifrelenmiş video için e-posta gönderildi: {Email}",
                videoMeta?.UploadedUser?.Email
            );
        }

        // Video akış endpointi
        [HttpGet("video-metadata-stream/{videoId}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> StreamVideo(
            [FromRoute] int videoId,
            [FromQuery] string key
        )
        {
            if (string.IsNullOrEmpty(key))
                return BadRequest("Şifreleme anahtarı (key) gereklidir.");

            int userId = GetAuthenticatedId();

            var debtVideo = await _context.DebtVideoMetadatas
                .Include(dvm => dvm.VideoMetadata)
                .Include(dvm => dvm.Debt)
                .FirstOrDefaultAsync(dvm => dvm.VideoMetadataId == videoId);

            if (debtVideo?.VideoMetadata == null || debtVideo.Debt == null)
            {
                return NotFound("Video or associated debt not found.");
            }

            if (debtVideo.Debt.LenderId != userId && !User.IsInRole("Admin"))
            {
                return Forbid("You are not the lender for this debt and cannot view the video.");
            }

            if (debtVideo.Debt.Status != DebtStatusType.Defaulted && !User.IsInRole("Admin"))
            {
                return BadRequest("You can only view the video for defaulted debts.");
            }

            var video = debtVideo.VideoMetadata;
            if (video.Status != VideoStatusType.Encrypted || string.IsNullOrEmpty(video.EncryptedFilePath) || string.IsNullOrEmpty(video.EncryptionKeyHash))
            {
                return BadRequest("Video is not available for streaming or encryption details are missing.");
            }

            var providedKeyHash = _mediaEncryptionService.HashKey(key);
            if (providedKeyHash != video.EncryptionKeyHash)
            {
                return Unauthorized("Invalid encryption key.");
            }

            try
            {
                var decryptedStream = _mediaEncryptionService.GetDecryptedVideoStream(
                    video.EncryptedFilePath, key, video.EncryptionSalt, video.EncryptionIV);

                if (decryptedStream == null)
                {
                    return NotFound("Encrypted video file not found or could not be decrypted.");
                }

                _logger.LogInformation("User {UserId} started streaming video for debt {DebtId}", userId, debtVideo.DebtId);
                return File(decryptedStream, video.ContentType ?? "application/octet-stream", video.OriginalFileName ?? "video.mp4");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during video streaming for video ID {VideoId}.", videoId);
                return StatusCode(500, "An error occurred during video streaming.");
            }
        }
    }
}
