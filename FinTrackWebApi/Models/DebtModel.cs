using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    public class DebtModel
    {
        [Key]
        [Required]
        public int DebtId { get; set; }

        [Required]
        [ForeignKey("LenderId")]
        public int LenderId { get; set; }
        public virtual UserModel Lender { get; set; } = new UserModel();

        [Required]
        [ForeignKey("BorrowerId")]
        public int BorrowerId { get; set; }
        public virtual UserModel Borrower { get; set; } = new UserModel();

        [Required]
        [ForeignKey("VideoMetadataId")]
        public int VideoMetadataId { get; set; }
        public virtual VideoMetadataModel VideoMetadata { get; set; } = new VideoMetadataModel();

        [Required]
        [ForeignKey("CurrencyId")]
        public int CurrencyId { get; set; }
        public virtual CurrencyModel? CurrencyModel { get; set; }

        [Required]
        [Column("Amount")]
        public decimal Amount { get; set; }

        [Required]
        [Column("Description")]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column("CreateAtUtc")]
        [DataType(DataType.DateTime)]
        public DateTime CreateAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("UpdatedAtUtc")]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("DueDateUtc")]
        [DataType(DataType.DateTime)]
        public DateTime DueDateUtc { get; set; }

        [Required]
        [Column("Status")]
        [MaxLength(100)]
        public DebtStatus Status { get; set; } = DebtStatus.PendingBorrowerAcceptance;

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
    }

    public enum DebtStatus
    {
        PendingBorrowerAcceptance,  // Borç Alan Onayı Bekliyor
        PendingOperatorApproval,    // Operatör Onayı Bekliyor (eğer varsa)
        Active,                     // Aktif Borç
        PaymentConfirmationPending, // Ödeme Onayı Bekliyor
        Paid,                       // Ödendi
        Defaulted,                  // Vadesi Geçmiş/Ödenmemiş
        RejectedByBorrower,         // Borç Alan Tarafından Reddedildi
        RejectedByOperator,         // Operatör Tarafından Reddedildi (eğer varsa)
        CancelledByLender           // Borç Veren Tarafından İptal Edildi
    }
}
