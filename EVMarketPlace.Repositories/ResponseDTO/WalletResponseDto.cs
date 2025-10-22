namespace EVMarketPlace.Repositories.ResponseDTO
{
    /// <summary>
    /// DTO cho thông tin ví
    /// </summary>
    public class WalletResponseDto
    {
        public Guid WalletId { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// DTO cho kết quả nạp tiền
    /// </summary>
    public class WalletTopUpResponseDto
    {
        public Guid WalletId { get; set; }
        public decimal AmountTopUp { get; set; }
        public decimal NewBalance { get; set; }
        public decimal OldBalance { get; set; }
        public DateTime TopUpDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }
}