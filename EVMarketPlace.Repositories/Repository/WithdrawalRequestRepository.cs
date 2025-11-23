using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace EVMarketPlace.Repositories.Repository
{
    public class WithdrawalRequestRepository : GenericRepository<WithdrawalRequest>
    {
        /// <summary>
        /// Lấy yêu cầu rút tiền theo UserId (có include navigation properties)
        /// </summary>
        public async Task<List<WithdrawalRequest>> GetByUserIdAsync(Guid userId)
        {
            return await _context.WithdrawalRequests
                .Include(w => w.User)
                .Include(w => w.Wallet)
                .Include(w => w.ProcessedByNavigation) 
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả yêu cầu (Admin) - có include navigation properties
        /// </summary>
        public async Task<List<WithdrawalRequest>> GetAllWithIncludeAsync()
        {
            return await _context.WithdrawalRequests
                .Include(w => w.User)
                .Include(w => w.Wallet)
                .Include(w => w.ProcessedByNavigation) 
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy yêu cầu theo Id - có include navigation properties
        /// </summary>
        public async Task<WithdrawalRequest?> GetByIdWithIncludeAsync(Guid withdrawalId)
        {
            return await _context.WithdrawalRequests
                .Include(w => w.User)
                .Include(w => w.Wallet)
                    .ThenInclude(wallet => wallet!.User)
                .Include(w => w.ProcessedByNavigation) 
                .FirstOrDefaultAsync(w => w.WithdrawalId == withdrawalId);
        }

        /// <summary>
        /// Lấy yêu cầu theo trạng thái
        /// </summary>
        public async Task<List<WithdrawalRequest>> GetByStatusAsync(string status)
        {
            return await _context.WithdrawalRequests
                .Include(w => w.User)
                .Include(w => w.Wallet)
                .Include(w => w.ProcessedByNavigation) 
                .Where(w => w.Status == status)
                .OrderByDescending(w => w.RequestedAt)
                .ToListAsync();
        }
    }
}