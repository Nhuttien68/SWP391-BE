using EVMarketPlace.Repositories.RequestDTO.Posts;
using EVMarketPlace.Repositories.ResponseDTO.Posts;
using EVMarketPlace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using EVMarketPlace.Repositories.Entity;

namespace EVMarketPlace.Services.Implements
{


    public class PostService : IPostService 
    {
        private readonly EvMarketplaceContext _db;

        public PostService(EvMarketplaceContext db) => _db = db;

        // Tạo mới Post, set mặc định IsActive=true nếu không truyền
        public async Task<PostDto> CreateAsync(PostCreateRequest req, CancellationToken ct = default)
        {
            var post = new Post
            {
                PostId = Guid.NewGuid(),
                UserId = req.UserId,
                Type = req.Type,
                Title = req.Title,
                Description = req.Description,
                Price = req.Price,
                IsActive = req.IsActive ?? true,   // mặc định true
                CreatedAt = DateTime.UtcNow         // dùng UTC cho đồng nhất
            };

            await _db.Set<Post>().AddAsync(post, ct);
            await _db.SaveChangesAsync(ct);

            return ToDto(post);
        }

        //Lấy 1 post theo Id (read-only)
        public async Task<PostDto?> GetByIdAsync(Guid postId, CancellationToken ct = default)
        {
            var p = await _db.Set<Post>()
                             .AsNoTracking()
                             .FirstOrDefaultAsync(x => x.PostId == postId, ct);

            return p is null ? null : ToDto(p);
        }

        // Lấy danh sách Post (phân trang cơ bản + filter nhẹ)
        public async Task<IReadOnlyList<PostDto>> GetPagedAsync(
            int page = 1, int pageSize = 10, string? keyword = null, string? type = null, CancellationToken ct = default)
        {
            // Giới hạn phân trang
            page = Math.Max(page, 1);
            pageSize = Math.Max(pageSize, 10);

            var query = _db.Set<Post>().AsNoTracking();

            // Lọc theo từ khóa
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Title.Contains(keyword) || p.Description.Contains(keyword));

            // Lọc theo loại post
            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(p => p.Type == type);

            // Lấy danh sách
            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return posts.Select(ToDto).ToList();
        }


        // Cập nhật Post (partial update: gửi gì sửa nấy)
        public async Task<PostDto> UpdateAsync(PostUpdateRequest req, CancellationToken ct = default)
        {
            var post = await _db.Set<Post>().FirstOrDefaultAsync(p => p.PostId == req.PostId, ct);
            if (post == null)
                throw new KeyNotFoundException("Post không tồn tại");

            post.Type = req.Type ?? post.Type;
            post.Title = req.Title ?? post.Title;
            post.Description = req.Description ?? post.Description;
            post.Price = req.Price ?? post.Price;
            post.IsActive = req.IsActive ?? post.IsActive;

            _db.Entry(post).State = EntityState.Modified; // bắt EF ghi UPDATE chắc chắn
            await _db.SaveChangesAsync(ct);

            return ToDto(post);
        }




        //Xóa Post theo Id
        public async Task<bool> DeleteAsync(Guid postId, CancellationToken ct = default)
        {
            var post = await _db.Set<Post>().FirstOrDefaultAsync(p => p.PostId == postId, ct);
            if (post == null) return false;

            _db.Set<Post>().Remove(post);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        //Map Entity -> DTO
        private static PostDto ToDto(Post p) => new PostDto
        {
            PostId = p.PostId,
            UserId = p.UserId,
            Type = p.Type,
            Title = p.Title,
            Description = p.Description,
            Price = p.Price,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        };
    }
}
