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
    public class FavoriteRepositori :GenericRepository<Favorite>
    {
        public async Task<IEnumerable<Favorite>> GetAllFavoriteWithPostInforAsync(Guid userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)               
                .Include(f => f.Post)
                    .ThenInclude(p => p.PostImages)            
                .OrderByDescending(f => f.CreatedAt)        
                .ToListAsync();
        }
        public async Task<Favorite?> GetFavoriteByIdAsync(Guid favoriteId, Guid userId)
        {
            return await _context.Favorites
                .Include(f => f.Post)
                    .ThenInclude(p => p.PostImages)
                .FirstOrDefaultAsync(f => f.FavoriteId == favoriteId && f.UserId == userId);
        }
    }
}
