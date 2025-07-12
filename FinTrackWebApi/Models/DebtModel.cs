using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class DebtModel
    {
        public int Id { get; set; }
        public int LenderId { get; set; }
        public virtual UserModel? Lender { get; set; }
        public int BorrowerId { get; set; }
        public virtual UserModel? Borrower { get; set; }
        public int CurrencyId { get; set; }
        public virtual CurrencyModel? CurrencyModel { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public DateTime DueDateUtc { get; set; }
        public DebtStatusType Status { get; set; }

        //[Required]
        //[ForeignKey("OperatorId")]
        //public int OperatorId { get; set; }
        //public virtual OperatorModel Operator { get; set; } = new OperatorModel();

        //[Required]
        //[Column("OperatorApprovalDateUtc")]
        //[DataType(DataType.DateTime)]
        //public DateTime? OperatorApprovalDateUtc { get; set; } = null;

        //[Required]
        //[Column("BorrowerAcceptanceDateUtc")]
        //[DataType(DataType.DateTime)]
        //public DateTime? BorrowerAcceptanceDateUtc { get; set; } = null;

        //[Required]
        //[Column("PaymentConfirmationDateUtc")]
        //[DataType(DataType.DateTime)]
        //public DateTime? PaymentConfirmationDateUtc { get; set; } = null;

        public virtual ICollection<DebtVideoMetadataModel> DebtVideoMetadatas { get; set; } =
            new List<DebtVideoMetadataModel>();
    }
}
