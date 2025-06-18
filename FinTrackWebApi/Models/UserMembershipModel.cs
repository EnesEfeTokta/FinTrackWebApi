using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("UserMemberships")]
    public class UserMembershipModel
    {
        [Required]
        [Key]
        public int UserMembershipId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual UserModel? User { get; set; }

        [Required]
        [ForeignKey("MembershipPlan")]
        public int MembershipPlanId { get; set; }
        public virtual MembershipPlanModel? Plan { get; set; }

        [Required]
        [Column("StartDate")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Column("EndDate")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [Column("Status")]
        public MembershipStatus Status { get; set; } = MembershipStatus.PendingPayment;

        [Required]
        [Column("AutoRenew")]
        public bool AutoRenew { get; set; } = true;

        [Column("CancellationDate")]
        [DataType(DataType.Date)]
        public DateTime? CancellationDate { get; set; }

        public virtual ICollection<PaymentModel> Payments { get; set; } = new List<PaymentModel>();
    }

    public enum MembershipStatus
    {
        PendingPayment = 0,
        Active = 1,
        Expired = 2,
        Cancelled = 3,
        FailedPayment = 4
    }
}
