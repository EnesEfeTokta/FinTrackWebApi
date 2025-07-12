using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.CategoriesDtos
{
    public class CategoryUpdateDto
    {
        public string Name { get; set; } = null!;
        public TransactionCategoryType Type { get; set; }
    }
}
