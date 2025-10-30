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
    }
}