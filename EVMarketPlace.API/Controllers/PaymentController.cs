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
        [ApiExplorerSettings(IgnoreApi = true)]  // ẨN khỏi Swagger
        public async Task<IActionResult> PaymentReturn()
        {
            var response = await _paymentService.ProcessPaymentReturnAsync(Request.Query, HttpContext);
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}