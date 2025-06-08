using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models
{
    public class EmployeeDepartmentModel
    {
        [Key]
        [Required]
        public int EmployeeDepartmentId { get; set; }

        [Required]
        [ForeignKey("EmployeeId")]
        public int EmployeeId { get; set; }
        public EmployeesModel? Employee { get; set; }

        [Required]
        [ForeignKey("DepartmentId")]
        public int DepartmentId { get; set; }
        public DepartmentModel? Department { get; set; }

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

        [Column("Notes")]
        [MaxLength(1000)]
        public string? Notes { get; set; } = null;
    }
}
