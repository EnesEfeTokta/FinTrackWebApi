using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Dtos
{
    public class BudgetCategoryDto
    {
        public int BudgetId { get; set; }

        public int CategoryId { get; set; }

        public decimal AllocatedAmount { get; set; }
    }

    public class BudgetCategoryCreateDto
    {
        [Required]
        public int BudgetId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public decimal AllocatedAmount { get; set; }
    }

    public class BudgetCategoryUpdateDto
    {
        [Required]
        public int BudgetId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public decimal AllocatedAmount { get; set; }
    }
}
