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

        public UserService(UserRepository userRepository, IConfiguration configuration, IOtpService otpService, IEmailSender emailSender)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _otpService = otpService;
            _emailSender = emailSender;
        }

        public async Task<BaseResponse> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                throw new NotFoundException("Không tìm thấy tài khoản với email này.");

            var isValidOtp = _otpService.VerifyOtp(request.Email, request.Otp);
            if (!isValidOtp)
                throw new NotFoundException("OTP không hợp lệ hoặc đã hết hạn.");

            user.PasswordHash = HashPassword.HashPasswordSHA256(request.NewPassWord);
            await _userRepository.UpdateAsync(user);

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Đặt lại mật khẩu thành công."
            };
        }

        public async Task<BaseResponse> CreateAccount(CreateAccountRequest request)
        {
            //var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            //if (existingUser != null)
            //    throw new NotFoundException("Email đã được đăng ký.");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = HashPassword.HashPasswordSHA256(request.Password),
                Role = RoleEnum.USER.ToString(),
                CreatedAt = DateTime.UtcNow,
                Status = UserStatusEnum.INACTIVE.ToString()
            };

            await _userRepository.CreateAsync(user);

            var otp = _otpService.GenerateAndSaveOtp(user.Email);
            var html = $"<p>Mã OTP của bạn là: <b>{otp}</b></p><p>Mã sẽ hết hạn sau 5 phút.</p>";

            try
            {
                await _emailSender.SendEmailAsync(user.Email, "Mã OTP xác thực tài khoản", html);
            }
            catch (Exception ex)
            {
                throw new NotFoundException("Không thể gửi email OTP.");
            }

            var response = new CreateAccountRespone
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Status = user.Status
            };

            return new BaseResponse
            {
                Status = StatusCodes.Status201Created.ToString(),
                Message = "Tạo tài khoản thành công.",
                Data = response
            };
        }

        public async Task<BaseResponse> LoginAsync(LoginRequest request)
        {
            var account = await _userRepository.GetAccountAsync(request);
            if (account == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status400BadRequest.ToString(),
                    Message = "Đăng nhập thất bại. Vui lòng kiểm tra lại email và mật khẩu."
                };
            }

            var token = GenerateJSONWebToken(account);

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Đăng nhập thành công.",
                Data = new LoginResponse
                {
                    AccountId = account.UserId,
                    FullName = account.FullName,
                    Email = account.Email,
                    Phone = account.Phone,
                    Token = token,
                    Status = account.Status,
                    Role = account.Role
                }
            };
        }



        public async Task<BaseResponse> VerifyOtpActiveAccountAsync(string email, string otp)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new NotFoundException("Không tìm thấy tài khoản với email này.");

            var isValidOtp = _otpService.VerifyOtp(email, otp);
            if (!isValidOtp)
                throw new NotFoundException("OTP không hợp lệ hoặc đã hết hạn.");

            user.Status = UserStatusEnum.ACTIVE.ToString();
            await _userRepository.UpdateAsync(user);

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Xác minh OTP thành công. Tài khoản đã được kích hoạt.",
                Data = new
                {
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.Status
                }
            };
        }


        private string GenerateJSONWebToken(User account)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // dùng ð? l?y key
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); //m? hóa 

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"]
                    , _configuration["Jwt:Audience"]
                    , new Claim[]
                    {
                   new(ClaimTypes.Name, account.FullName),
                   new Claim(JwtRegisteredClaimNames.NameId, account.UserId.ToString()),
                   new (ClaimTypes.Role, account.Role.ToString())
                    },
                    expires: DateTime.Now.AddMinutes(120),// set th?i gian h?t h?n
                    signingCredentials: credentials
                    );


            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }

        public async Task<BaseResponse> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Không tìm thấy tài khoản với email này."
                };
            }

            var otp = _otpService.GenerateAndSaveOtp(email, 5);
            var html = $"<p>Mã OTP để đặt lại mật khẩu: <b>{otp}</b></p><p>Mã sẽ hết hạn sau 5 phút.</p>";

            try
            {
                await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu", html);
            }
            catch (Exception ex)
            {
                throw new NotFoundException("Không thể gửi email OTP.");
            }

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Mã OTP đã được gửi đến email của bạn."
            };
        }

        public async Task<BaseResponse> ResendOtpAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new NotFoundException("Email không tồn tại trong hệ thống.");

            if (user.Status == UserStatusEnum.ACTIVE.ToString())
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status400BadRequest.ToString(),
                    Message = "Tài khoản đã được kích hoạt."
                };
            }

            var otp = _otpService.GenerateAndSaveOtp(email, 5);
            var html = $"<p>Mã OTP để kích hoạt tài khoản: <b>{otp}</b></p><p>Mã sẽ hết hạn sau 5 phút.</p>";

            try
            {
                await _emailSender.SendEmailAsync(email, "Mã OTP kích hoạt tài khoản", html);
            }
            catch (Exception ex)
            {
                throw new NotFoundException("Không thể gửi lại email OTP.");
            }

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Mã OTP mới đã được gửi đến email của bạn."
            };
        }

        public async Task<BaseResponse> GetAllUser()
        {
            try
            {
                var users = await _userRepository.GetAllActiveUsersAsync();

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Get active users successfully.",
                    Data = users
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "Error: " + ex.Message
                };
            }
        }

        public async Task<BaseResponse> CountUser()
        {
            try
            {
                int total = await _userRepository.CountActiveUsersAsync();

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Count active users successfully.",
                    Data = total
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "Error: " + ex.Message
                };
            }

        }

        public async Task<BaseResponse> UpdateUserStatusAsync(Guid userId, string status)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "User not found"
                    };
                }

                // Validate status
                if (status != UserStatusEnum.ACTIVE.ToString() && status != UserStatusEnum.INACTIVE.ToString())
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status400BadRequest.ToString(),
                        Message = "Invalid status. Must be ACTIVE or INACTIVE"
                    };
                }

                user.Status = status;
                await _userRepository.UpdateAsync(user);

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = $"User status updated to {status} successfully",
                    Data = new { UserId = user.UserId, Status = user.Status }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "Error updating user status: " + ex.Message
                };
            }
        }
    }
}
