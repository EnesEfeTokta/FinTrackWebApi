using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.User;

namespace FinTrackWebApi.Models.Debt
{
    public class DebtModel
    {
        public int Id { get; set; }
        public int LenderId { get; set; }
        public virtual UserModel? Lender { get; set; }
        public int BorrowerId { get; set; }
        public virtual UserModel? Borrower { get; set; }
        public decimal Amount { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public DateTime DueDateUtc { get; set; }
        public DebtStatusType Status { get; set; }
        public DateTime? PaidAtUtc { get; set; }
        public DateTime? OperatorApprovalAtUtc { get; set; }
        public DateTime? BorrowerApprovalAtUtc { get; set; }
        public DateTime? PaymentConfirmationAtUtc { get; set; }

        public virtual ICollection<DebtVideoMetadataModel> DebtVideoMetadatas { get; set; } =
            new List<DebtVideoMetadataModel>();
    }
}
