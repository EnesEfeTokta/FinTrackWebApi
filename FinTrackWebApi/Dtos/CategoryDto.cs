using System.ComponentModel.DataAnnotations;
using FinTrackWebApi.Models;

namespace FinTrackWebApi.Dtos
{
    public class CategoryDto
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public CategoryType Type { get; set; }
    }
}
