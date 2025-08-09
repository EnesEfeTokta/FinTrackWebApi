namespace FinTrackWebApi.Dtos.UserMembershipDtos
{
    public class SubscriptionRequestDto
    {
        public int PlanId { get; set; }
        public bool AutoRenew { get; set; } = true;
    }
}
