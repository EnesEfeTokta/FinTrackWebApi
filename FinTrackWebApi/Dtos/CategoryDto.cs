using System.ComponentModel.DataAnnotations;
using FinTrackWebApi.Models;
using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public TransactionCategoryType Type { get; set; }
    }

    public class CategoryCreateDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public TransactionCategoryType Type { get; set; }
    }

    public class CategoryUpdateDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public TransactionCategoryType Type { get; set; }
    }
}
