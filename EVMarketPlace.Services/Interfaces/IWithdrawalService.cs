using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using System.Security.Claims;

namespace EVMarketPlace.Services.Interfaces
{
    //interface trong .net core dùng để khai báo các phương thức mà một lớp phải triển khai
    //trong trường hợp này interface định nghĩa các phương thức liên quan đến việc rút tiền từ ví người dùng

    public interface IWithdrawalService
    {
        Task<BaseResponse> CreateWithdrawalRequestAsync(ClaimsPrincipal user, CreateWithdrawalRequest request);
        Task<BaseResponse> GetMyWithdrawalRequestsAsync(ClaimsPrincipal user);
        Task<BaseResponse> GetAllWithdrawalRequestsAsync(ClaimsPrincipal user);
        Task<BaseResponse> ApproveWithdrawalAsync(ClaimsPrincipal user, Guid withdrawalId, string? adminNote);
        Task<BaseResponse> RejectWithdrawalAsync(ClaimsPrincipal user, Guid withdrawalId, string adminNote);
    }
}