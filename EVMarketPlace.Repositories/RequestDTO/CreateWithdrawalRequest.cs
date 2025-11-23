
namespace EVMarketPlace.Repositories.RequestDTO
{
    public class CreateWithdrawalRequest
    {
        public decimal Amount { get; set; }
        public string BankName { get; set; } = null!;
        public string BankAccountNumber { get; set; } = null!;
        public string BankAccountName { get; set; } = null!;
        public string? Note { get; set; }
    }
}
