using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using System.Security.Claims;

namespace EVMarketPlace.Services.Interfaces
{
    public interface ITransactionService
    {
        // thanh toan gio hang
        Task<BaseResponse> CreateCartTransactionAsync(ClaimsPrincipal user, CreateCartTransactionRequest request);
        // thanh toan don le
        Task<BaseResponse> CreateTransactionAsync(ClaimsPrincipal user, CreateTransactionRequest request);
        Task<BaseResponse> GetTransactionByIdAsync(ClaimsPrincipal user, Guid transactionId);
        Task<BaseResponse> GetMyPurchasesAsync(ClaimsPrincipal user);
        Task<BaseResponse> GetMySalesAsync(ClaimsPrincipal user);
        //Task<BaseResponse> UpdateTransactionStatusAsync(ClaimsPrincipal user, UpdateTransactionStatusRequest request);
        Task<BaseResponse> CancelTransactionAsync(ClaimsPrincipal user, Guid transactionId);
        Task<BaseResponse> GetAllTransactionsAsync(ClaimsPrincipal user);
    }
}