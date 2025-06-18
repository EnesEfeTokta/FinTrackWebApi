using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("MembershipPlans")]
    public class MembershipPlanModel
    {
        [Required]
        [Key]
        public int MembershipPlanId { get; set; }

        [Required]
        [Column("Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column("Price")]
        public decimal Price { get; set; }

        [Required]
        [Column("Currency")]
        public string Currency { get; set; } = string.Empty;

        [Required]
        [Column("BillingCycle")]
        public string BillingCycle { get; set; } = string.Empty;

        [Column("DurationInDays")]
        public int? DurationInDays { get; set; }

        [Column("Features")]
        public string? Features { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserMembershipModel> UserMemberships { get; set; } =
            new List<UserMembershipModel>();
    }
}
