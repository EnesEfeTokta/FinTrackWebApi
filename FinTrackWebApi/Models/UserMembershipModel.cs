using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class UserMembershipModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel? User { get; set; }
        public int MembershipPlanId { get; set; }
        public virtual MembershipPlanModel? Plan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public MembershipStatusType Status { get; set; }
        public bool AutoRenew { get; set; } = true;
        public DateTime? CancellationDate { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PaymentModel> Payments { get; set; } = new List<PaymentModel>();
    }
}
