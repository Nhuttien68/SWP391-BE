using EVMarketPlace.Repositories.ResponseDTO;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IWalletService
    {
        Task<BaseResponse> CreateWalletAsync();
        Task<BaseResponse> GetWalletAsync();

        // Nạp tiền vào ví (dùng cho VNPay callback)
        Task<BaseResponse> TopUpWalletAsync(decimal amount, string transactionId, string paymentMethod, Guid userId);

        Task<BaseResponse> WithdrawWalletAsync(decimal amount);
        Task<decimal> GetBalanceAsync();
    }
}