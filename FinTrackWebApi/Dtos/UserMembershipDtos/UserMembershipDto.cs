namespace FinTrackWebApi.Dtos.UserMembershipDtos
{
    public class UserMembershipDto
    {
        public int Id { get; set; }
        public int PlanId { get; set; }
        public string? PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public bool AutoRenew { get; set; }
    }
}
