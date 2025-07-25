using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.NotificationDtos;
using FinTrackWebApi.Enums;
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

                var notificationsFromDb = await _context
                    .Notifications.Where(n => n.UserId == userId)
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

                return Ok(notificationsFromDb);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching notifications for user {UserId}",
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while fetching notifications.");
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

        [HttpPut("{Id}")]
        public async Task<IActionResult> NotificationRead(int Id)
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == Id && n.UserId == userId);
                if (notification == null)
                {
                    _logger.LogWarning(
                        "Notification with ID {NotificationId} not found for user ID: {NotificationId}",
                        Id,
                        userId
                    );
                    return NotFound("Notification not found.");
                }

                notification.IsRead = true;
                notification.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully updated notification with ID {NotificationId} for user ID: {UserId}",
                    Id,
                    userId
                );
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating notification with ID {NotificationId} for user ID: {UserId}",
                    Id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while updating notification.");
            }
        }

        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteNotification(int Id)
        {
            int userId = GetAuthenticatedUserId();

            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == Id && n.UserId == userId);
                if (notification == null)
                {
                    _logger.LogWarning(
                        "Notification with ID {NotificationId} not found for user ID: {UserId}",
                        Id,
                        userId
                    );
                    return NotFound($"Notification with ID {Id} not found.");
                }

                if (notification.Type == NotificationType.Success ||
                    notification.Type == NotificationType.Info)
                {
                    _context.Notifications.Remove(notification);
                }
                if (notification.Type == NotificationType.Error ||
                    notification.Type == NotificationType.Warning &&
                    notification.IsRead == true)
                {
                    _context.Notifications.Remove(notification);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Successfully deleted notification with ID {NotificationId} for user ID: {UserId}",
                    Id,
                    userId
                );
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting notification with ID {NotificationId} for user ID: {UserId}",
                    Id,
                    GetAuthenticatedUserId()
                );
                return StatusCode(500, "Internal server error while deleting the notification.");
            }
        }
    }
}
