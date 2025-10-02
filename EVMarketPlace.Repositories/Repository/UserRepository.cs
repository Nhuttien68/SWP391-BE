using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.Repository
{
    public class UserRepository :GenericRepository<User>
    {
        public async Task<User> GetAccountAsync(LoginRequest loginRequest)
        {
            return await _context.Users.FirstOrDefaultAsync(a => a.Email == loginRequest.Email &&
            a.PasswordHash.Equals(HashPassword.HashPasswordSHA256(loginRequest.Password)) &&
            a.IsActive == true );
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email );
            
        }
    }
}
