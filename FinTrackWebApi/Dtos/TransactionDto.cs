using System.ComponentModel.DataAnnotations;
using FinTrackWebApi.Models;

namespace FinTrackWebApi.Dtos
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;

        public CategoryType CategoryType { get; set; }

        public int UserId { get; set; }
        public string Username { get; set; } = null!;

        public int AccountId { get; set; }
        public string AccountName { get; set; } = null!;

        public decimal Amount { get; set; }

        public DateTime TransactionDateUtc { get; set; }

        public string Description { get; set; } = null!;
    }

    public class TransactionCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, (double)decimal.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDateUtc { get; set; }

        [Required]
        public string Description { get; set; } = null!;
    }

    public class TransactionUpdateDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, (double)decimal.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime TransactionDateUtc { get; set; }

        [Required]
        public string Description { get; set; } = null!;
    }
}
