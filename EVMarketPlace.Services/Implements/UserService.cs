using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository, IConfiguration configuration)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public async Task<CreateAccountRespone> CreateAccount(CreateAccountRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email đã được đăng ký!");
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
                IsActive = true
            };
           await _userRepository.CreateAsync(user);// lưu vào DB
            var response = new CreateAccountRespone
            {
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                IsActive = user.IsActive
            };
            return response;
        }

        public async Task<BaseRespone> LoginAsync(LoginRequest request)
        {
            var account =  await _userRepository.GetAccountAsync(request);
            if (account == null)
            {
                throw new InvalidOperationException("Đăng nhập thất bại. Vui lòng kiểm tra lại email và mật khẩu.");

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
    }
}
