using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.Repository
{
    public class AuctionRepository : GenericRepository<Auction>
    {
        public async Task<Auction?> GetAuctionWithBidsAsync(Guid auctionId)
        {
            return await _context.Auctions
                .Include(a => a.Post)
                    .ThenInclude(p => p.PostImages)
                .Include(a => a.Post)
                    .ThenInclude(p => p.Vehicle)
                .Include(a => a.Post)
                    .ThenInclude(p => p.Battery)
                .Include(a => a.AuctionBids)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(a => a.AuctionId == auctionId);
        }

        public async Task<List<Auction>> GetExpiredAuctionsAsync()
        {
            return await _context.Auctions
                .Include(a => a.AuctionBids)
                .Include(a => a.Post)
                .Where(a => a.Status == "Active" && a.EndTime <= DateTime.Now)
                .ToListAsync();
        }

        public async Task<List<Auction>> GetActiveAuctionsAsync()
        {
            var now = DateTime.Now;
            return await _context.Auctions
                .Include(a => a.Post)
                    .ThenInclude(p => p.PostImages)
                .Include(a => a.Post)
                    .ThenInclude(p => p.Vehicle)
                .Include(a => a.Post)
                    .ThenInclude(p => p.Battery)
                .Include(a => a.AuctionBids)
                    .ThenInclude(b => b.User)
                .Where(a => a.Status == "Active" && a.EndTime > now)
                .OrderBy(a => a.EndTime)
                .ToListAsync();
        }

        public async Task AddBidAsync(AuctionBid bid)
        {
            await _context.AuctionBids.AddAsync(bid);
            await _context.SaveChangesAsync();
        }

        public async Task<Post?> GetPostByIdAsync(Guid postId)
        {
            return await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<bool> PostHasAuctionAsync(Guid postId)
        {
            // Chỉ kiểm tra đấu giá Active hoặc đang Processing
            // Cho phép tạo lại nếu đấu giá cũ đã Ended (không có winner) hoặc Failed
            return await _context.Auctions
                .AnyAsync(a => a.PostId == postId && 
                              (a.Status == "Active" || a.Status == "Processing"));
        }
    }
}
