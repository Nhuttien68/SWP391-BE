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
                throw new NotFoundException("Không tìm thấy tài khoản với email này.");
            }

            var isValidOtp = _otpService.VerifyOtp(request.Email, request.Otp);
            if (!isValidOtp)
            {
                throw new NotFoundException("OTP không hợp lệ hoặc đã hết hạn.");
            }

            user.PasswordHash = HashPassword.HashPasswordSHA256(request.NewPassWord);
            await _userRepository.UpdateAsync(user);

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
                UserId = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = HashPassword.HashPasswordSHA256(request.Password),
                Role = RoleEnum.USER.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsActive = false,
            };

            await _userRepository.CreateAsync(user);

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

            return new BaseRespone 
            { 
                Status = StatusCodes.Status201Created.ToString(),
                Message = "Tạo tài khoản thành công",
                Data = response
            };
        }