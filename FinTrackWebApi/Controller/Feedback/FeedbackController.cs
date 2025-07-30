using FinTrackWebApi.Data;
using FinTrackWebApi.Dtos.DebtDtos;
using FinTrackWebApi.Dtos.FeedbackDtos;
using FinTrackWebApi.Models.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinTrackWebApi.Controller.Feedback
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "User,Admin")]
    public class FeedbackController : ControllerBase
    {
        private readonly ILogger<FeedbackController> _logger;
        private readonly MyDataContext _context;

        public FeedbackController(ILogger<FeedbackController> logger, MyDataContext context)
        {
            _logger = logger;
            _context = context;
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

        [HttpGet]
        public async Task<IActionResult> GetFeedbacks()
        {
            int userId = GetAuthenticatedId();
            try
            {
                var feedbacks = await _context.Feedbacks
                    .AsNoTracking().Where(f => f.UserId == userId)
                    .Select(f => new FeedbackDto
                    {
                        Id = f.Id,
                        Subject = f.Subject,
                        Description = f.Description,
                        Type = f.Type,
                        SavedFilePath = f.SavedFilePath,
                        CreatedAtUtc = f.CreatedAtUtc,
                        UpdatedAtUtc = f.UpdatedAtUtc
                    })
                    .ToListAsync();
                if (feedbacks == null || feedbacks.Count == 0)
                {
                    _logger.LogInformation(
                        "No feedbacks found for user with ID {UserId}.",
                        userId
                    );
                    return NotFound("No feedbacks found for the user.");
                }

                _logger.LogInformation(
                    "Retrieved {FeedbackCount} feedbacks for user with ID {UserId}.",
                    feedbacks.Count,
                    userId
                );
                return Ok(feedbacks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user feedbacks.");
                return StatusCode(500, "Internal server error while retrieving feedbacks.");
            }
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetFeedback(int Id)
        {
            int userId = GetAuthenticatedId();
            try
            {
                var feedback = await _context.Feedbacks
                    .AsNoTracking().Where(f => f.UserId == userId)
                    .Select(f => new FeedbackDto 
                    {
                        Id = f.Id,
                        Subject = f.Subject,
                        Description = f.Description,
                        Type = f.Type,
                        SavedFilePath = f.SavedFilePath,
                        CreatedAtUtc = f.CreatedAtUtc,
                        UpdatedAtUtc = f.UpdatedAtUtc
                    }).FirstOrDefaultAsync();
                if (feedback == null)
                {
                    _logger.LogInformation(
                        "No feedback found for user with ID {UserId}.",
                        userId
                    );
                    return NotFound("No feedback found for the user.");
                }

                _logger.LogInformation(
                    "Retrieved feedback with ID {FeedbackId} for user with ID {UserId}.",
                    Id,
                    GetAuthenticatedId()
                );
                return Ok(feedback);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving feedback by ID.");
                return StatusCode(500, "Internal server error while retrieving feedback.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackCreateDto feedback)
        {
            if (feedback == null)
            {
                return BadRequest("Feedback data is required.");
            }
            try
            {
                int userId = GetAuthenticatedId();

                var newFeadback = new FeedbackModel
                {
                    UserId = userId,
                    Subject = feedback.Subject,
                    Description = feedback.Description,
                    Type = feedback.Type,
                    SavedFilePath = feedback.SavedFilePath,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.Feedbacks.Add(newFeadback);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Feedback successfully created.");
                return Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating feedback for user {UserId}",
                    GetAuthenticatedId()
                );
                return StatusCode(500, "An error occurred while creating the feedback.");
            }
        }
    }
}
