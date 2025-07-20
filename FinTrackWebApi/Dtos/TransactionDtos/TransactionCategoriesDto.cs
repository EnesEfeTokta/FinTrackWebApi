using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.TransactionDtos
{
    public class TransactionCategoriesDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public TransactionCategoryType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
