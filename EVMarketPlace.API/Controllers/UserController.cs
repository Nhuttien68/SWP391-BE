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
        private readonly IEmailSender _emailSender;


        public UserController(IUserService userService, IEmailSender emailSender  )
        {
            _userService = userService;     
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
        {
            var response = await _userService.CreateAccount(request);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPost("verify-email-active-account")]
        public async Task<IActionResult> VerifyEmail(string email, string otp)
        {
            var response = await _userService.VerifyOtpActiveAccountAsync(email, otp);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPut("change-password")]
        public async Task<IActionResult> ResetPassword(ChangePasswordRequest request)
        {
            var response = await _userService.ChangePasswordAsync(request);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _userService.LoginAsync(request);
            return StatusCode(int.Parse(response.Status), response);
        }
        //[HttpPost("resend-otp")]
        //public async Task<IActionResult>ResentOpt(string gmail)
        //{
        //    var otp = await _userService.ResendOtpAsync(gmail);
        //    return StatusCode(int.Parse(otp.Status), otp);

        //}

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var response = await _userService.ForgotPasswordAsync(email);
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}
