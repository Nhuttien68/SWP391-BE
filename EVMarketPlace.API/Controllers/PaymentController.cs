using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// Frontend gọi endpoint này để lấy payment URL
        /// </summary>
        [HttpGet("create")]
        [Authorize]
        public async Task<IActionResult> CreatePayment(
            [FromQuery] decimal amount,
            [FromQuery] string info,
            [FromQuery] string? orderId = null)
        {
            var response = await _paymentService.CreatePaymentUrlAsync(HttpContext, amount, info, orderId);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// VNPay redirect callback (AUTO - Không cần gọi từ Frontend)
        /// VNPay tự động redirect về endpoint này sau thanh toán
        /// HIDDEN từ Swagger - Chỉ để test với VNPay
        /// </summary>
        [HttpGet("return")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> PaymentReturn()
        {
            var response = await _paymentService.ProcessPaymentReturnAsync(Request.Query, HttpContext);

            // Parse response status
            var statusCode = int.Parse(response.Status);
            var success = statusCode == 200;

            // Frontend URL
            var frontendUrl = "http://localhost:5173/payment-return";

            // Lấy thông tin từ VNPay query params
            var vnpResponseCode = Request.Query["vnp_ResponseCode"].ToString();
            var vnpAmount = Request.Query["vnp_Amount"].ToString();
            var vnpOrderInfo = Request.Query["vnp_OrderInfo"].ToString();
            var vnpTransactionNo = Request.Query["vnp_TransactionNo"].ToString();

            if (success && vnpResponseCode == "00")
            {
                // Thanh toán thành công - redirect với params VNPay
                var redirectUrl = $"{frontendUrl}?vnp_ResponseCode={vnpResponseCode}&vnp_Amount={vnpAmount}&vnp_OrderInfo={Uri.EscapeDataString(vnpOrderInfo)}&vnp_TransactionNo={vnpTransactionNo}";
                return Redirect(redirectUrl);
            }
            else
            {
                // Thanh toán thất bại
                var redirectUrl = $"{frontendUrl}?vnp_ResponseCode={vnpResponseCode}&message={Uri.EscapeDataString(response.Message)}";
                return Redirect(redirectUrl);
            }
        }
    }
}