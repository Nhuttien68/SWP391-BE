using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Exception;
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
        public PostRepository(EvMarketplaceContext context) : base(context)
        {
        }
        public async Task<bool> UpdateStatusSoldAsync(Guid postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.PostId == postId);
            if (post == null) return false;

            post.Status = PostStatusEnum.SOLD.ToString();
            await _context.SaveChangesAsync();
            return true;
        }

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
        public async Task UpdateBatteryAsync(Post post)
        {
            var existingPost = await _context.Posts
                .Include(p => p.Battery)
                .Include(p => p.PostImages)
                .FirstOrDefaultAsync(p => p.PostId == post.PostId);

            if (existingPost == null)
                throw new NotFoundException("Post not found");

            // Gán từng trường và báo cho EF biết là có thay đổi
            if (existingPost.Title != post.Title)
                existingPost.Title = post.Title;

            if (existingPost.Description != post.Description)
                existingPost.Description = post.Description;

            if (existingPost.Price != post.Price)
                existingPost.Price = post.Price;

            // Battery
            if (existingPost.Battery != null && post.Battery != null)
            {
                existingPost.Battery.BrandId = post.Battery.BrandId;
                existingPost.Battery.Capacity = post.Battery.Capacity;
                existingPost.Battery.Condition = post.Battery.Condition;
            }
            else if (post.Battery != null)
            {
                post.Battery.PostId = existingPost.PostId;
                await _context.Batteries.AddAsync(post.Battery);
            }

            // Force EF hiểu rằng entity này có thay đổi
            _context.Entry(existingPost).State = EntityState.Modified;
            if (existingPost.Battery != null)
                _context.Entry(existingPost.Battery).State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }
        public async Task UpdateVehicleAsync(Post post)
        {
            var existingPost = await _context.Posts
                .Include(p => p.Vehicle)
                .Include(p => p.PostImages)
                .FirstOrDefaultAsync(p => p.PostId == post.PostId);

            if (existingPost == null)
                throw new NotFoundException("Post not found");

            // ==== Update Post ====
            existingPost.Title = post.Title;
            existingPost.Description = post.Description;
            existingPost.Price = post.Price;

            // ==== Update Vehicle ====
            if (existingPost.Vehicle != null && post.Vehicle != null)
            {
                existingPost.Vehicle.BrandId = post.Vehicle.BrandId;
                existingPost.Vehicle.Model = post.Vehicle.Model;
                existingPost.Vehicle.Year = post.Vehicle.Year;
                existingPost.Vehicle.Mileage = post.Vehicle.Mileage;

                // Cái này rất quan trọng để EF hiểu rằng entity Vehicle đã thay đổi
                _context.Entry(existingPost.Vehicle).State = EntityState.Modified;
            }

            // ==== Update ảnh nếu có ====
            if (post.PostImages != null && post.PostImages.Any())
            {
                existingPost.PostImages = post.PostImages;
            }

            _context.Entry(existingPost).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId)
        {
            return await _context.Posts
                 .Include(p => p.User)
                .Include(p => p.PostImages)
                .Include(p => p.User)
                .Include(p => p.Vehicle)
                .ThenInclude(v => v.Brand)
                .Include(p => p.Battery)
                .ThenInclude(b => b.Brand)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Post>> GetAllPostWithPennding()
        {
            return await _context.Posts
               .Include(p => p.PostImages)
               .Include(p => p.User)
               .Include(p => p.Vehicle)
               .ThenInclude(v => v.Brand)
               .Include(p => p.Battery)
               .ThenInclude(b => b.Brand)
               .Where(p => p.Status == PostStatusEnum.PENNDING.ToString())
               .ToListAsync();
        }
        // Đếm tổng số bài đăng
        public async Task<int> CountPostsByStatusAsync(PostStatusEnum status)
        {
            return await _context.Posts
                .CountAsync(p => p.Status == status.ToString());
        }
        // Lấy bài đăng theo ngày và trạng thái
        public async Task<List<Post>> GetPostsByDateAndStatusAsync(int day, int month, int year, PostStatusEnum status)
        {
            return await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.User)
                .Include(p => p.Vehicle).ThenInclude(v => v.Brand)
                .Include(p => p.Battery).ThenInclude(b => b.Brand)
                .Where(p =>
                    p.CreatedAt.HasValue &&
                    p.CreatedAt.Value.Day == day &&
                    p.CreatedAt.Value.Month == month &&
                    p.CreatedAt.Value.Year == year &&
                    p.Status == status.ToString()
                )
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        // Lấy bài đăng theo tháng và trạng thái
        public async Task<List<Post>> GetPostsByMonthAndStatusAsync(int month, int year, PostStatusEnum status)
        {
            return await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.User)
                .Include(p => p.Vehicle).ThenInclude(v => v.Brand)
                .Include(p => p.Battery).ThenInclude(b => b.Brand)
                .Where(p =>
                    p.CreatedAt.HasValue &&
                    p.CreatedAt.Value.Month == month &&
                    p.CreatedAt.Value.Year == year &&
                    p.Status == status.ToString()
                )
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        // Lấy bài đăng theo năm và trạng thái
        public async Task<List<Post>> GetPostsByYearAndStatusAsync(int year, PostStatusEnum status)
        {
            return await _context.Posts
                .Include(p => p.PostImages)
                .Include(p => p.User)
                .Include(p => p.Vehicle).ThenInclude(v => v.Brand)
                .Include(p => p.Battery).ThenInclude(b => b.Brand)
                .Where(p =>
                    p.CreatedAt.HasValue &&
                    p.CreatedAt.Value.Year == year &&
                    p.Status == status.ToString()
                )
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }


    }
}
