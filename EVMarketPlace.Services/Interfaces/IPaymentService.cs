using EVMarketPlace.Repositories.ResponseDTO;
using Microsoft.AspNetCore.Http;

namespace EVMarketPlace.Services.Interfaces
{
    // Interface cho dịch vụ thanh toán VNPay
    public interface IPaymentService
    {
        // Tạo URL thanh toán VNPay
        Task<BaseResponse> CreatePaymentUrlAsync(HttpContext context, decimal amount, string orderInfo, string? orderId = null);
        // Xử lý callback return từ VNPay
        Task<BaseResponse> ProcessPaymentReturnAsync(IQueryCollection query, HttpContext context);
    }
}