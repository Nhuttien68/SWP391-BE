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
                .Include(a => a.AuctionBids)
                .FirstOrDefaultAsync(a => a.AuctionId == auctionId);
        }

        public async Task<List<Auction>> GetExpiredAuctionsAsync()
        {
            return await _context.Auctions
                .Include(a => a.AuctionBids)
                .Include(a => a.Post)
                .Where(a => a.Status == "Active" && a.EndTime <= DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task<List<Auction>> GetActiveAuctionsAsync()
        {
            return await _context.Auctions
                .Include(a => a.Post)
                .Where(a => a.Status == "Active")
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
            return await _context.Auctions.AnyAsync(a => a.PostId == postId);
        }
    }
}
