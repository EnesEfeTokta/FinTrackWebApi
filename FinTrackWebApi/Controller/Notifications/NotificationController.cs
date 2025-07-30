using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.NotificationDtos;
using FinTrackWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Notifications
{
    [Route("[controller]")]
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
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var notificationsFromDb = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAtUtc)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        MessageHead = n.MessageHead,
                        MessageBody = n.MessageBody,
                        NotificationType = n.Type,
                        CreatedAt = n.CreatedAtUtc,
                        IsRead = n.IsRead,
                    })
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("{Count} notifications have been delivered to user ID {UserId}.", notificationsFromDb.Count, userId);
                return Ok(notificationsFromDb);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching notifications for user.");
                return StatusCode(500, "Internal server error while fetching notifications.");
            }
        }

        [HttpPost("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

                if (notification == null)
                {
                    return NotFound($"Notification with ID {id} not found for this user.");
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAtUtc = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("Notification {NotificationId} marked as read for user {UserId}.", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                int affectedRows = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(b => b.IsRead, true)
                        .SetProperty(b => b.ReadAtUtc, DateTime.UtcNow));

                _logger.LogInformation("{Count} unread notifications marked as read for user {UserId}.", affectedRows, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification(
            [FromBody] NotificationCreateDto notificationDto
        )
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                var notification = new NotificationModel
                {
                    UserId = userId,
                    MessageHead = notificationDto.MessageHead,
                    MessageBody = notificationDto.MessageBody,
                    Type = notificationDto.NotificationType,
                    CreatedAtUtc = DateTime.UtcNow,
                    IsRead = false,
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetNotifications),
                    new { id = notification.Id },
                    notificationDto
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating notification for user {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while creating notification.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                int userId = GetAuthenticatedUserId();
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

                if (notification == null)
                {
                    return NotFound($"Notification with ID {id} not found.");
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted notification {NotificationId} for user {UserId}.", id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId}.", id);
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpDelete("clear-all")]
        public async Task<IActionResult> ClearAllNotifications()
        {
            try
            {
                int userId = GetAuthenticatedUserId();

                int affectedRows = await _context.Notifications
                    .Where(n => n.UserId == userId)
                    .ExecuteDeleteAsync();

                _logger.LogInformation("Cleared {Count} notifications for user {UserId}.", affectedRows, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all notifications for user.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}
