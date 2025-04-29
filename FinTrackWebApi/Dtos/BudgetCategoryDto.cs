using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Dtos
{
    public class BudgetCategoryDto
    {
        [Required]
        public int BudgetId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public decimal AllocatedAmount { get; set; }
    }
}
