using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.CategoriesDtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public TransactionCategoryType Type { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
