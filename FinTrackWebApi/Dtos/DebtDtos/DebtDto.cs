using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.DebtDtos
{
    public class DebtDto
    {
        public int Id { get; set; }
        public int LenderId { get; set; }
        public string LenderName { get; set; } = null!;
        public string LenderEmail { get; set; } = null!;
        public string? LenderProfilePicture { get; set; }
        public int BorrowerId { get; set; }
        public string BorrowerName { get; set; } = null!;
        public string BorrowerEmail { get; set; } = null!;
        public string? BorrowerProfilePicture { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public DateTime DueDateUtc { get; set; }
        public string Description { get; set; } = null!;
        public DebtStatusType Status { get; set; }
        public DateTime CreateAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public DateTime? PaidAtUtc { get; set; }
        public DateTime? OperatorApprovalAtUtc { get; set; }
        public DateTime? BorrowerApprovalAtUtc { get; set; }
    }
}
