using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrackWebApi.Models.Empoyee
{
    public class DepartmentModel
    {
        [Key]
        [Required]
        public int DepartmentId { get; set; }

        [Required]
        [Column("Name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("Description")]
        [MaxLength(500)]
        public string? Description { get; set; } = null;

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

        public ICollection<EmployeeDepartmentModel> EmployeeDepartments { get; set; } =
            new List<EmployeeDepartmentModel>();
    }
}
