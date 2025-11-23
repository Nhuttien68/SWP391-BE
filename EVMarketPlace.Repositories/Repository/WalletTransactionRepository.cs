using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace EVMarketPlace.Repositories.Repository
{
    public class WalletTransactionRepository : GenericRepository<WalletTransaction>
    {

        // Lấy lịch sử giao dịch theo UserId

        public async Task<List<WalletTransaction>> GetByUserIdAsync(Guid userId)
        {
            return await _context.WalletTransactions
                .Include(wt => wt.Wallet)
                .Where(wt => wt.Wallet!.UserId == userId)
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync();
        }

        // Lấy giao dịch theo loại
        public async Task<List<WalletTransaction>> GetByTypeAsync(Guid userId, string transactionType)
        {
            return await _context.WalletTransactions
                .Include(wt => wt.Wallet)
                .Where(wt => wt.Wallet!.UserId == userId && wt.TransactionType == transactionType)
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync();
        }

        // Tạo log giao dịch ví
        public async Task<WalletTransaction> CreateLogAsync(WalletTransaction log)
        {
            await _context.WalletTransactions.AddAsync(log);
            await _context.SaveChangesAsync();
            return log;
        }

        // Lấy giao dịch trong khoảng thời gian
        public async Task<List<WalletTransaction>> GetByDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _context.WalletTransactions
                .Include(wt => wt.Wallet)
                .Where(wt => wt.Wallet!.UserId == userId
                          && wt.CreatedAt >= startDate
                          && wt.CreatedAt <= endDate)
                .OrderByDescending(wt => wt.CreatedAt)
                .ToListAsync();
        }
    }
}