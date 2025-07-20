using FinTrackWebApi.Enums;
using FinTrackWebApi.Models.User;

namespace FinTrackWebApi.Models.Membership
{
    public class PaymentModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual UserModel? User { get; set; }
        public int? UserMembershipId { get; set; }
        public virtual UserMembershipModel? UserMembership { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public BaseCurrencyType Currency { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public PaymentStatusType Status { get; set; }
        public string? GatewayResponse { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
