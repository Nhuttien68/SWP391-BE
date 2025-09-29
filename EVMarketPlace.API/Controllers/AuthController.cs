using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    public class AuthController : Controller
    {
        private readonly IOtpService _otpService;
        private readonly IEmailSender _emailSender;
        public AuthController(IOtpService otpService, IEmailSender emailSender)
        {
            _otpService = otpService;
            _emailSender = emailSender;
        }
        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            var otp = _otpService.GenerateAndSaveOtp(email, 5);

            var html = $"<p>Mã OTP của bạn là: <b>{otp}</b></p><p>Mã sẽ hết hạn sau 5 phút.</p>";
            await _emailSender.SendEmailAsync(email, "Mã OTP xác thực", html);

            return Ok(new { message = "OTP đã được gửi vào email." });
        }

        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var success = _otpService.VerifyOtp(dto.Email, dto.Code);
            if (!success)
                return BadRequest(new { message = "OTP không hợp lệ hoặc đã hết hạn." });

            return Ok(new { message = "Xác thực OTP thành công!" });
        }
    }

    public class VerifyOtpDto
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
}

