using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Models
{
    public class MembershipPlanModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public BaseCurrencyType? Currency { get; set; }
        public BillingCycleType? BillingCycle { get; set; }
        public int? DurationInDays { get; set; }
        public string? Features { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserMembershipModel> UserMemberships { get; set; } =
            new List<UserMembershipModel>();
    }
}
