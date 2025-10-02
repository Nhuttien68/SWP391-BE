using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _userService;


        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        /// <summary>
        /// API dùng để đăng ký tài khoản người dùng mới.
        /// </summary>
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
    }
}
