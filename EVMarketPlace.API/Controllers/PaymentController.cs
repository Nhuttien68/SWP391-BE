using EVMarketPlace.Services.Implements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentController : ControllerBase
    {
        private readonly VnPayService _vnPayService;

        public PaymentController(VnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        /// <summary>
        /// Tạo URL thanh toán VNPay
        /// </summary>
        /// <param name="amount">Số tiền thanh toán (VND)</param>
        /// <param name="info">Thông tin đơn hàng</param>
        /// <param name="orderId">Mã đơn hàng (optional)</param>
        [HttpGet("create")]
        [Authorize]
        public IActionResult CreatePayment(
            [FromQuery] decimal amount,
            [FromQuery] string info,
            [FromQuery] string? orderId = null)
        {
            var response = _vnPayService.CreatePaymentUrl(HttpContext, amount, info, orderId);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// VNPay redirect callback
        /// </summary>
        [HttpGet("return")]
        [AllowAnonymous]
        public IActionResult PaymentReturn()
        {
            var response = _vnPayService.ProcessPaymentReturn(Request.Query);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// VNPay IPN callback (server-to-server) - VNPay gọi trực tiếp
        /// </summary>
        [HttpGet("ipn")]
        [AllowAnonymous]
        public IActionResult PaymentIpn()
        {
            var response = _vnPayService.ProcessIpnCallback(Request.Query);
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}