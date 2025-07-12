using FinTrackWebApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Dtos
{
    public class AccountDto
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = null!;

        public AccountType Type { get; set; }

        public bool IsActive { get; set; } = true;

        public decimal Balance { get; set; } = 0;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAtUtc { get; set; } = null;
    }

    public class AccountCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public AccountType Type { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Balance must be greater than zero.")]
        public decimal Balance { get; set; } = 0;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAtUtc { get; set; } = null;
    }

    public class AccountUpdateDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public AccountType Type { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Balance must be greater than zero.")]
        public decimal Balance { get; set; } = 0;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAtUtc { get; set; } = DateTime.MinValue;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
