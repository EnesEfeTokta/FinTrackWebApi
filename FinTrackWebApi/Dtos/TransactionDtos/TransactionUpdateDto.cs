namespace FinTrackWebApi.Dtos.TransactionDtos
{
    public class TransactionUpdateDto
    {
        public int CategoryId { get; set; }
        public int AccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDateUtc { get; set; }
        public string? Description { get; set; }
    }
}
