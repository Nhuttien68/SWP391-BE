namespace EVMarketPlace.Repositories.ResponseDTO
{
    public class TransactionListItemDTO
    {
        public Guid TransactionId { get; set; }
        public string PostTitle { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? PostImageUrl { get; set; }
    }
}