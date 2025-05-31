using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Dtos
{
    public class DebtCreateDto
    {
        [Required]
        [EmailAddress]
        public string BorrowerEmail { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "The debt amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "TRY";

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
