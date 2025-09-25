using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
