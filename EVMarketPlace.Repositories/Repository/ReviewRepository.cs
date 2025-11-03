using EVMarketPlace.Repositories.Entity;
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
    }
}
