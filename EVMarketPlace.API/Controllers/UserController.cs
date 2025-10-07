using EVMarketPlace.Repositories.Exception;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Implements;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _userService;
        private readonly IOtpService _otpService;
        private readonly IEmailSender _emailSender;


        public UserController(IUserService userService, IOtpService otpService,IEmailSender emailSender  )
        {
            _userService = userService;
            _otpService = otpService;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<BaseRespone> Create(CreateAccountRequest request)
        {
            return await _userService.CreateAccount(request);
        }
        [HttpPost("verify-email-active-account")]
        public async Task<BaseRespone> VerifyEmail(string email, string otp)
        {
            return await _userService.VerifyOtpActiveAccountAsync(email, otp);
        }
        [HttpPut("change-password")]
        public async Task<BaseRespone> ResetPassword(ChangePasswordRequest request)
        {
            return await _userService.ChangePasswordAsync(request);
        }
        [HttpPost("login")]
        public async Task<BaseRespone> Login(LoginRequest request)
        {
            return await _userService.LoginAsync(request);
        }
        [HttpPost("resend-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string email)
        {
            var user = await _userService.VerifyOtpActiveAccountAsync(email, "");
            if (user == null)
            {
                throw new NotFoundException("Email không tồn tại.");
            }
            var otp = _otpService.GenerateAndSaveOtp(email, 5);

            var html = $"<p>Mã OTP của bạn là: <b>{otp}</b></p><p>Mã sẽ hết hạn sau 5 phút.</p>";
            await _emailSender.SendEmailAsync(email, "Mã OTP xác thực", html);

            return Ok(new { message = "OTP đã được gửi vào email." });
        }

        [HttpPost("forgot-password")]
        public async Task<BaseRespone> ForgotPassword([FromBody] string email)
        {
            return await _userService.ForgotPasswordAsync(email);
        }
    }
}
