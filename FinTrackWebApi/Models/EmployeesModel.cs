using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    [Table("Employees")]
    public class EmployeesModel
    {
        [Key]
        [Required]
        [Column("EmployeeId")]
        public int EmployeeId { get; set; }

        [Required]
        [Column("Name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("Email")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("Password")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Column("PhoneNumber")]
        [MaxLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Column("Position")]
        [MaxLength(100)]
        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.None;

        [Required]
        [Column("HireDate")]
        [MaxLength(255)]
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("Salary")]
        public decimal Salary { get; set; }

        [Required]
        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Column("CreatedAtUtc")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("UpdatedAtUtc")]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("CreatedBy")]
        [MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        [Column("UpdatedBy")]
        [MaxLength(100)]
        public string UpdatedBy { get; set; } = string.Empty;

        [Column("ProfilePictureUrl")]
        [MaxLength(500)]
        public string? ProfilePictureUrl { get; set; } = null;

        [Column("Notes")]
        [MaxLength(1000)]
        public string? Notes { get; set; } = null;

        public ICollection<EmployeeDepartmentModel> EmployeeDepartments { get; set; } =
            new List<EmployeeDepartmentModel>();
    }

    public enum EmployeeStatus
    {
        VideoApproval,
        CEO,
        CTO,
        Support,
        DataAnalyst,
        Admin,
        None,
    }
}
