using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace FinTrackWebApi.Models
{
    [Table("Users")]
    public class UserModel : IdentityUser<int>
    {
        [Required]
        [Column("ProfilePicture")]
        public string ProfilePicture { get; set; } =
            "https://img.freepik.com/free-vector/blue-circle-with-white-user_78370-4707.jpg?semt=ais_hybrid&w=740";

        [Required]
        [Column("CreateAt")]
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public virtual UserAppSettingsModel? AppSettings { get; set; }
        public virtual UserNotificationSettingsModel? NotificationSettings { get; set; }

        public virtual ICollection<BudgetModel> Budgets { get; set; } = new List<BudgetModel>();
        public virtual ICollection<CategoryModel> Categories { get; set; } =
            new List<CategoryModel>();
        public virtual ICollection<OtpVerificationModel> OtpVerifications { get; set; } =
            new List<OtpVerificationModel>();
        public virtual ICollection<TransactionModel> Transactions { get; set; } =
            new List<TransactionModel>();
        public virtual ICollection<AccountModel> Accounts { get; set; } = new List<AccountModel>();

        public virtual ICollection<UserMembershipModel> UserMemberships { get; set; } =
            new List<UserMembershipModel>();
        public virtual ICollection<PaymentModel> Payments { get; set; } = new List<PaymentModel>();

        public virtual ICollection<NotificationModel> Notifications { get; set; } =
            new List<NotificationModel>();

        public virtual ICollection<DebtModel> DebtsAsLender { get; set; } = new List<DebtModel>();
        public virtual ICollection<DebtModel> DebtsAsBorrower { get; set; } = new List<DebtModel>();
        public virtual ICollection<VideoMetadataModel> UploadedVideos { get; set; } =
            new List<VideoMetadataModel>();
    }
}
