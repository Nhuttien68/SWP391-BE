using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace EVMarketPlace.Repositories.Repository
{
    public class TransactionRepository : GenericRepository<Transaction>
    {

        // Tạo giao dịch mới
        public async Task<Transaction> CreateAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        // Lấy giao dịch theo Id
        public async Task<Transaction?> GetByIdAsync(Guid transactionId)
        {
            return await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Post)
                    .ThenInclude(p => p.PostImages)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        // Lấy danh sách giao dịch của Buyer
        public async Task<List<Transaction>> GetByBuyerIdAsync(Guid buyerId)
        {
            return await _context.Transactions
                .Include(t => t.Seller)
                .Include(t => t.Post)
                    .ThenInclude(p => p.PostImages)
                .Where(t => t.BuyerId == buyerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // Lấy danh sách giao dịch của Seller
        public async Task<List<Transaction>> GetBySellerIdAsync(Guid sellerId)
        {
            return await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Post)
                    .ThenInclude(p => p.PostImages)
                .Where(t => t.SellerId == sellerId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // Lấy tất cả giao dịch (Admin)
        public async Task<List<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Post)
                    .ThenInclude(p => p.PostImages)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        // Cập nhật giao dịch
        public async Task<Transaction> UpdateAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }
        // ✅ Hàm lấy transaction có kèm Post
        public async Task<Transaction?> GetTransactionWithPostAsync(Guid transactionId)
        {
            return await _context.Transactions
                .Include(t => t.Post)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        // ✅ Hàm lấy transaction có kèm Seller (nếu cần)
        public async Task<Transaction?> GetTransactionWithSellerAsync(Guid transactionId)
        {
            return await _context.Transactions
                .Include(t => t.Seller)
                .Include(t => t.Post)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }
        // ✅ Hàm lấy tất cả transaction sắp xếp theo ngày tạo giảm dần
        public async Task<List<Transaction>> GetAllSortedByDateDescAsync()
        {
            return await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Post)
                    .ThenInclude(p => p.PostImages)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        // ✅ Hàm lấy tất cả transaction trong khoảng thời gian nhất định
        public async Task<List<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Post)
                    .ThenInclude(p => p.PostImages)
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        // ✅ Hàm lấy tất cả transaction trong  năm 
        public async Task<List<Transaction>> GetByYearAsync(int year)
        {
            return await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Post)
                    .ThenInclude(p => p.PostImages)
                .Where(t => t.CreatedAt.Value.Year == year)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        // ✅ Hàm lấy tất cả transaction trong tháng của năm
        public async Task<List<Transaction>> GetByMonthAsync(int month, int year)
        {
            return await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Post)
                    .ThenInclude(p => p.PostImages)
                .Where(t => t.CreatedAt.Value.Month == month &&
                            t.CreatedAt.Value.Year == year)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }
        // ✅ Hàm lấy tất cả transaction trong ngày cụ thể
        public async Task<List<Transaction>> GetByDateAsync(int day, int month, int year)
        {
            return await _context.Transactions
                .Include(t => t.Buyer)
                .Include(t => t.Seller)
                .Include(t => t.Post)
                    .ThenInclude(p => p.PostImages)
                .Where(t => t.CreatedAt.Value.Day == day &&
                            t.CreatedAt.Value.Month == month &&
                            t.CreatedAt.Value.Year == year)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

    }
}