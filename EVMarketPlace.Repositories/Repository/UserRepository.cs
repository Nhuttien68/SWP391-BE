using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
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
            return await _context.Users.FirstOrDefaultAsync(a =>
            a.Email == loginRequest.Email &&
            a.PasswordHash == HashPassword.HashPasswordSHA256(loginRequest.Password) &&
            a.Status == UserStatusEnum.ACTIVE.ToString()
        ); ;
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email );
            
        }
        public async Task<IEnumerable<User>> GetAllActiveUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Wallet)
                .Where(u => u.Status == UserStatusEnum.ACTIVE.ToString())
                .ToListAsync();
        }
        public async Task<int> CountActiveUsersAsync()
        {
            return await _context.Users
                .CountAsync(u => u.Status == UserStatusEnum.ACTIVE.ToString());
        }

        public async Task<User?> GetAdminUserAsync()
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "ADMIN" && u.Status == UserStatusEnum.ACTIVE.ToString());
        }
    }
}
