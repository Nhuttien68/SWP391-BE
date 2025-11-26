using EVMarketPlace.Repositories.ResponseDTO;
using System.Security.Claims;

namespace EVMarketPlace.Services.Interfaces
{
    public interface ISystemSettingService
    {
        Task<BaseResponse> GetCommissionRateAsync();
        Task<BaseResponse> UpdateCommissionRateAsync(ClaimsPrincipal user, decimal rate);
        Task<BaseResponse> GetAllPaymentSettingsAsync(ClaimsPrincipal user);
        Task<BaseResponse> GetCommissionReportAsync(ClaimsPrincipal user, DateTime startDate, DateTime endDate);
    }
}