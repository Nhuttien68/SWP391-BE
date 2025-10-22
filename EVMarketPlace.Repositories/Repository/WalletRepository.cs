using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.Repository
{
    public class WalletRepository : GenericRepository<Wallet>
    {
        /// <summary>
        /// Lấy ví của user theo userId
        /// </summary>
        public async Task<Wallet?> GetWalletByUserIdAsync(Guid userId)
        {
            return await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Status != "DELETED");
        }

        /// <summary>
        /// Lấy ví theo WalletId
        /// </summary>
        public async Task<Wallet?> GetWalletByIdAsync(Guid walletId)
        {
            return await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WalletId == walletId && w.Status != "DELETED");
        }

        /// <summary>
        /// Cập nhật balance ví (atomic operation)
        /// </summary>
        public async Task<(bool Success, decimal NewBalance)> TryUpdateBalanceAsync(
            Guid walletId,
            decimal amountChange)
        {
            try
            {
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.WalletId == walletId && w.Status != "DELETED");

                if (wallet == null)
                    return (false, 0);

                var oldBalance = wallet.Balance ?? 0;
                var newBalance = oldBalance + amountChange;

                // ❌ Không cho phép số dư âm
                if (newBalance < 0)
                    return (false, oldBalance);

                // ✅ Cập nhật
                wallet.Balance = newBalance;
                wallet.LastUpdated = DateTime.UtcNow;

                _context.Entry(wallet).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return (true, newBalance);
            }
            catch
            {
                return (false, 0);
            }
        }

        /// <summary>
        /// Kiểm tra ví có tồn tại không
        /// </summary>
        public async Task<bool> WalletExistsAsync(Guid userId)
        {
            return await _context.Wallets
                .AnyAsync(w => w.UserId == userId && w.Status != "DELETED");
        }

        /// <summary>
        /// Lấy balance hiện tại
        /// </summary>
        public async Task<decimal> GetBalanceAsync(Guid userId)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            return wallet?.Balance ?? 0;
        }
    }
}
