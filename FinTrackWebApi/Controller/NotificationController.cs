using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos;
using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace FinTrackWebApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class NotificationController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(MyDataContext context, ILogger<NotificationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token.");
            }
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<NotificationDto>> GetNotifications()
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var notificationsFromDb = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAtUtc)
                    .Select(n => new NotificationDto
                    {
                        Id = n.NotificationId,
                        MessageHead = n.MessageHead,
                        MessageBody = n.MessageBody,
                        NotificationType = n.NotificationType,
                        CreatedAt = n.CreatedAtUtc,
                        IsRead = n.IsRead
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(notificationsFromDb);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications for user {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while fetching notifications.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] NotificationDto notificationDto)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var notification = new NotificationModel
                {
                    UserId = userId,
                    MessageHead = notificationDto.MessageHead,
                    MessageBody = notificationDto.MessageBody,
                    NotificationType = notificationDto.NotificationType,
                    CreatedAtUtc = DateTime.UtcNow,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                notificationDto.Id = notification.NotificationId;
                notificationDto.CreatedAt = notification.CreatedAtUtc;
                return CreatedAtAction(nameof(GetNotifications), new { id = notification.NotificationId }, notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, "Internal server error while creating notification.");
            }
        }
    }
}
