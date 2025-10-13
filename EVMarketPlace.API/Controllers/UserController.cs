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
        //[HttpPost("resend-otp")]
        //public async Task<IActionResult> SendOtp([FromBody] ResendOtpRequest request)
        //{
        //    try
        //    {
        //        var result = await _userService.ResendOtpAsync(request.Email);
                
        //        if (result.Status == "200")
        //        {
        //            return Ok(result);
        //        }
        //        else
        //        {
        //            return BadRequest(result);
        //        }
        //    }
        //    catch (NotFoundException ex)
        //    {
        //        return BadRequest(new BaseRespone
        //        {
        //            Status = "404",
        //            Message = ex.Message
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new BaseRespone
        //        {
        //            Status = "500",
        //            Message = "Có lỗi xảy ra khi gửi OTP: " + ex.Message
        //        });
        //    }
        //}

        [HttpPost("forgot-password")]
        public async Task<BaseRespone> ForgotPassword([FromBody] string email)
        {
            return await _userService.ForgotPasswordAsync(email);
        }
    }
}
