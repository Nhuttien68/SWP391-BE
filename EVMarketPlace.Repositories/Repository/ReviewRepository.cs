using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.Repository
{
    public class ReviewRepository : GenericRepository<Review>
    {
        private readonly EvMarketplaceContext _context;
        public ReviewRepository(EvMarketplaceContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(Guid transactionId, string targetType)
        {
            return await _context.Reviews
                .AnyAsync(r => r.TransactionId == transactionId && r.ReviewTargetType == targetType);
        }
        public async Task<List<Review>> GetReviewsByPostIdAsync(Guid postId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Post).ThenInclude(p => p.User)
                .Include(r => r.Post).ThenInclude(p => p.PostImages)
                .Where(r => r.PostId == postId && r.ReviewTargetType == ReviewTypeEnum.PostReview.ToString())
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByUserIdAsync(Guid userId)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.ReviewedUser)
                .Where(r => r.ReviewedUserId == userId && r.ReviewTargetType == ReviewTypeEnum.SellerReview.ToString())
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
