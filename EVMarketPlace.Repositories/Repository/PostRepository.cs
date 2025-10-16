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
    public class PostRepository : GenericRepository<Post>
    {
        public async Task<IEnumerable<Post>> GetAllPostWithImageAsync()
        {
            return await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.User)
                .Include(p => p.Vehicle)
                .ThenInclude(v => v.Brand)
                .Include(p => p.Battery)
                .ThenInclude(b => b.Brand)
                .Where(p => p.Status == PostStatusEnum.APPROVED.ToString())
                .ToListAsync();
        }
        public async Task<Post> GetPostByIdWithImageAsync(Guid postId)
        {
            return await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.User)
                .Include(p => p.Vehicle)
                .ThenInclude(v => v.Brand)  
                .Include(p => p.Battery)
                .ThenInclude(b => b.Brand)
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }
     }
}
