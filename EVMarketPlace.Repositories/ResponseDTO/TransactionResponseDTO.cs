namespace EVMarketPlace.Repositories.ResponseDTO
{
    public class TransactionResponseDTO
    {
        public Guid TransactionId { get; set; }
        public Guid BuyerId { get; set; }
        public string BuyerName { get; set; }
        public Guid SellerId { get; set; }
        public string SellerName { get; set; }
        public Guid PostId { get; set; }
        public string PostTitle { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverAddress { get; set; }
        public string? Note { get; set; }
    }
}