using FinTrackWebApi.Enums;
using FinTrackWebApi.Models;

namespace FinTrackWebApi.Dtos.TransactionDtos
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public CategoryModel Category { get; set; } = null!;
        public AccountModel Account { get; set; } = null!;
        public decimal Amount { get; set; }
        public DateTime TransactionDateUtc { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
