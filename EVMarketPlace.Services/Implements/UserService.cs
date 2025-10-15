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
                throw new  NotFoundException("Kh�ng t?m th?y t�i kho?n v?i email n�y."); 
                
            }

            var isValidOtp = _otpService.VerifyOtp(request.Email, request.Otp);
            if (!isValidOtp)
            {
                throw new NotFoundException("OTP kh�ng h?p l? ho?c �? h?t h?n.");
             
            }

            user.PasswordHash = HashPassword.HashPasswordSHA256(request.NewPassWord);
            _userRepository.UpdateAsync(user).Wait();

            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "�?t l?i m?t kh?u th�nh c�ng."
            };
        }

        public async Task<BaseRespone> CreateAccount(CreateAccountRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
               
                throw new NotFoundException("Email �? ��?c ��ng k?!");
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
            await _userRepository.CreateAsync(user);// l�u v�o DB
            var otp = _otpService.GenerateAndSaveOtp(user.Email);
            var html = $"<p>M? OTP c?a b?n l�: <b>{otp}</b></p><p>M? s? h?t h?n sau 5 ph�t.</p>";
            await _emailSender.SendEmailAsync(user.Email, "M? OTP x�c th?c", html);
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
                    Message = "��ng nh?p th?t b?i. Vui l?ng ki?m tra l?i email v� m?t kh?u.",
                    Data = null
                };
            }
            
            var token = GenerateJSONWebToken(account);
            
            // Cho ph�p ��ng nh?p ngay c? khi ch�a k�ch ho?t
            // Nh�ng tr? v? th�ng tin �?c bi?t �? frontend x? l?
            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = account.IsActive ? "Login successfully" : "Login successfully but account not activated",
                Data = new LoginResponse
                {
                    AccountId = account.UserId,
                    FullName = account.FullName,
                    Email = account.Email,
                    Phone = account.Phone,
                    Token = token,
                    IsActive = account.IsActive
                }
            };
        }



        public async Task<BaseRespone> VerifyOtpActiveAccountAsync(string email, string opt)
        {
            // Ki?m tra user t?n t?i
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new NotFoundException( "Kh�ng t?m th?y t�i kho?n v?i email n�y.");
                
            }

            // Ki?m tra OTP
            var isValidOtp =  _otpService.VerifyOtp(email, opt);
            if (!isValidOtp)
            {
                throw new NotFoundException("OTP kh�ng h?p l? ho?c �? h?t h?n.");
               
            }

            // N?u OTP h?p l? -> k�ch ho?t t�i kho?n
            user.IsActive = true;
            await _userRepository.UpdateAsync(user);

            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "X�c minh OTP th�nh c�ng. T�i kho?n �? ��?c k�ch ho?t.",
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
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // d�ng �? l?y key
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); //m? h�a 

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

        public async Task<BaseRespone> ForgotPasswordAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Kh�ng t?m th?y t�i kho?n v?i email n�y."
                };
            }

            var otp = _otpService.GenerateAndSaveOtp(email, 5);

            var html = $"<p>M? OTP �? �?t l?i m?t kh?u: <b>{otp}</b></p><p>M? s? h?t h?n sau 5 ph�t.</p>";
            await _emailSender.SendEmailAsync(email, "�?t l?i m?t kh?u", html);

            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "M? OTP �? ��?c g?i �?n email c?a b?n."
            };
        }

        public async Task<BaseRespone> ResendOtpAsync(string email)
        {
            // Ki?m tra user t?n t?i
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new NotFoundException("Email kh�ng t?n t?i trong h? th?ng.");
            }

            // Ki?m tra user �? active ch�a
            if (user.IsActive)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status400BadRequest.ToString(),
                    Message = "T�i kho?n �? ��?c k�ch ho?t."
                };
            }

            // T?o v� g?i OTP m?i
            var otp = _otpService.GenerateAndSaveOtp(email, 5);
            var html = $"<p>M? OTP �? k�ch ho?t t�i kho?n: <b>{otp}</b></p><p>M? s? h?t h?n sau 5 ph�t.</p>";
            await _emailSender.SendEmailAsync(email, "M? OTP k�ch ho?t t�i kho?n", html);

            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "M? OTP �? ��?c g?i �?n email c?a b?n."
            };
        }
    }
}
