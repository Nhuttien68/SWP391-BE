using Azure;
using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Exception;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace EVMarketPlace.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly UserRepository _userRepository;
        private readonly IOtpService _otpService;
        private readonly IEmailSender _emailSender;

        public UserService(UserRepository userRepository, IConfiguration configuration, IOtpService otpService,IEmailSender emailSender)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _otpService = otpService;
            _emailSender = emailSender;
        }

        public async Task<BaseRespone> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = _userRepository.GetByEmailAsync(request.Email).Result;
            if (user == null)
            {
                throw new  NotFoundException("Không tìm thấy tài khoản với email này."); 
                
            }

            var isValidOtp = _otpService.VerifyOtp(request.Email, request.Otp);
            if (!isValidOtp)
            {
                throw new NotFoundException("OTP không hợp lệ hoặc đã hết hạn.");
             
            }

            user.PasswordHash = HashPassword.HashPasswordSHA256(request.NewPassWord);
            _userRepository.UpdateAsync(user).Wait();

            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Đặt lại mật khẩu thành công."
            };
        }

        public async Task<BaseRespone> CreateAccount(CreateAccountRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
               
                throw new NotFoundException("Email đã được đăng ký!");
            }
            var user = new User
            {
                UserId =  Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = HashPassword.HashPasswordSHA256(request.Password),
                Role = RoleEnum.USER.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsActive = false,
            };
            await _userRepository.CreateAsync(user);// lưu vào DB
            var otp = _otpService.GenerateAndSaveOtp(user.Email);
            var html = $"<p>Mã OTP của bạn là: <b>{otp}</b></p><p>Mã sẽ hết hạn sau 5 phút.</p>";
            await _emailSender.SendEmailAsync(user.Email, "Mã OTP xác thực", html);
            var response = new CreateAccountRespone
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive
            };
            return new BaseRespone { Status = StatusCodes.Status201Created.ToString(),
            Message= "Create accont successfully ",
            Data = response
            };
        }

        public async Task<BaseRespone> LoginAsync(LoginRequest request)
        {
            var account =  await _userRepository.GetAccountAsync(request);
            if (account == null)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status400BadRequest.ToString(),
                    Message = "Đăng nhập thất bại. Vui lòng kiểm tra lại email và mật khẩu.",
                    Data = null
                };
            }
            if (!account.IsActive)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status400BadRequest.ToString(),
                    Message = "Tài khoản chưa được kích hoạt. Vui lòng kiểm tra email để xác thực tài khoản.",
                    Data = null
                };
            }
            var token = GenerateJSONWebToken(account);
            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Login successfully",
                Data = new LoginResponse
                {
                    AccountId = account.UserId,
                    FullName = account.FullName,
                    Email = account.Email,
                    Token = token
                }
            };
        }



        public async Task<BaseRespone> VerifyOtpActiveAccountAsync(string email, string opt)
        {
            // Kiểm tra user tồn tại
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new NotFoundException( "Không tìm thấy tài khoản với email này.");
                
            }

            // Kiểm tra OTP
            var isValidOtp =  _otpService.VerifyOtp(email, opt);
            if (!isValidOtp)
            {
                throw new NotFoundException("OTP không hợp lệ hoặc đã hết hạn.");
               
            }

            // Nếu OTP hợp lệ -> kích hoạt tài khoản
            user.IsActive = true;
            await _userRepository.UpdateAsync(user);

            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Xác minh OTP thành công. Tài khoản đã được kích hoạt.",
                Data = new
                {
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.IsActive
                }
            };
        }

        private string GenerateJSONWebToken(User account)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // dùng để lấy key
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); //mã hóa 

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"]
                    , _configuration["Jwt:Audience"]
                    , new Claim[]
                    {
                   new(ClaimTypes.Name, account.FullName),
                   new Claim(JwtRegisteredClaimNames.NameId, account.UserId.ToString()),
                   new (ClaimTypes.Role, account.Role.ToString())
                    },
                    expires: DateTime.Now.AddMinutes(120),// set thời gian hết hạn
                    signingCredentials: credentials
                    );


            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }

        public async Task<BaseRespone> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Không tìm thấy tài khoản với email này."
                };
            }

            var otp = _otpService.GenerateAndSaveOtp(email, 5);

            var html = $"<p>Mã OTP để đặt lại mật khẩu: <b>{otp}</b></p><p>Mã sẽ hết hạn sau 5 phút.</p>";
            await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu", html);

            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Mã OTP đã được gửi đến email của bạn."
            };
        }
    }
}
