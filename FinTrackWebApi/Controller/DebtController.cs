using FinTrackWebApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Models;
using FinTrackWebApi.Services.EmailService;

namespace FinTrackWebApi.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User,Admin")]
    public class DebtController : ControllerBase
    {
        private readonly MyDataContext _context;
        private readonly ILogger<DebtController> _logger;
        private readonly IEmailSender _emailSender;

        public DebtController(MyDataContext context, ILogger<DebtController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Borç tekliflerini listeleme metodu.
        [HttpGet("offers")]
        public async Task<IActionResult> GetDebtOffersAsync()
        {
            try
            {
                var debtOffers = await _context.Debts
                    .Include(d => d.Lender)
                    .Include(d => d.Borrower)
                    .Include(d => d.VideoMetadata)
                    .Include(d => d.CurrencyModel)
                    .Where(d => d.Status == DebtStatus.PendingBorrowerAcceptance)
                    .ToListAsync();
                return Ok(debtOffers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Borç tekliflerini listeleme sırasında hata oluştu.");
                return StatusCode(500, "İç sunucu hatası.");
            }
        }

        // Borç teklifini kabul etme metodu.

    }
}
