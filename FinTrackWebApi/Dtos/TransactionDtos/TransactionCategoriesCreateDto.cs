using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.TransactionDtos
{
    public class TransactionCategoriesCreateDto
    {
        public string Name { get; set; } = null!;
        public TransactionCategoryType Type { get; set; }
    }
}
