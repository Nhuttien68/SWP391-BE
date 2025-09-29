using EVMarketPlace.Repositories.Context;
using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EVMarketPlace.Services.Implements
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _db;
        public PostService(AppDbContext db) => _db = db;

        // ----------------------------
        // GET LIST POSTS (paging + filter)
        // ----------------------------
        public async Task<(int total, IEnumerable<PostListItemDto> items)> GetListAsync(
            string? keyword, string? type, int page, int pageSize, CancellationToken ct = default)
        {
            // Chuẩn hóa tham số
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            IQueryable<Post> q = _db.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt);

            // Tìm theo keyword (dùng LIKE, giữ index, an toàn)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = $"%{keyword.Trim()}%";
                q = q.Where(p =>
                    EF.Functions.Like(p.Title, kw) ||
                    EF.Functions.Like(p.Description ?? string.Empty, kw));
            }


            if (!string.IsNullOrWhiteSpace(type))
            {
                var t = type.Trim();
                q = q.Where(p => p.Type == t);
            }

            var total = await q.CountAsync(ct);
            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostListItemDto
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    Type = p.Type,
                    Title = p.Title,
                    Price = p.Price,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync(ct);

            return (total, items);
        }

        // ----------------------------
        // GET DETAIL BY ID
        // ----------------------------
        public async Task<PostDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Posts.AsNoTracking()
                .Where(p => p.PostId == id)
                .Select(p => new PostDetailDto
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    Type = p.Type,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync(ct);
        }

        // ----------------------------
        // CREATE POST
        // ----------------------------
        public async Task<PostDetailDto> CreateAsync(CreatePostRequest req, CancellationToken ct = default)
        {
            // Guard input (null, Guid.Empty, chuỗi rỗng, price âm)
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (req.UserId == Guid.Empty) throw new ArgumentException("UserId is required.");
            if (string.IsNullOrWhiteSpace(req.Type)) throw new ArgumentException("Type is required.");
            if (string.IsNullOrWhiteSpace(req.Title)) throw new ArgumentException("Title is required.");
            if (req.Price < 0) throw new ArgumentException("Price must be >= 0.");

            // User phải tồn tại
            var userExists = await _db.Users.AnyAsync(u => u.UserId == req.UserId, ct);
            if (!userExists) throw new ArgumentException("UserId not found!");

            var post = new Post
            {
                PostId = Guid.NewGuid(),
                UserId = req.UserId,
                Type = req.Type.Trim(),
                Title = req.Title.Trim(),
                Description = req.Description,
                Price = req.Price,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _db.Posts.Add(post);
            await _db.SaveChangesAsync(ct);

            return new PostDetailDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Type = post.Type,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                IsActive = post.IsActive,
                CreatedAt = post.CreatedAt
            };
        }

        // ----------------------------
        // UPDATE POST
        // ----------------------------
        public async Task<PostDetailDto?> UpdateAsync(Guid id, UpdatePostRequest req, CancellationToken ct = default)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == id, ct);
            if (post is null) return null;

            if (!string.IsNullOrWhiteSpace(req.Type))
                post.Type = req.Type.Trim();

            if (!string.IsNullOrWhiteSpace(req.Title))
                post.Title = req.Title.Trim();


            if (req.Description is not null)
                post.Description = req.Description;

            if (req.Price.HasValue)
            {
                if (req.Price.Value < 0) throw new ArgumentException("Price must be >= 0.");
                post.Price = req.Price.Value;
            }

            if (req.IsActive.HasValue)
                post.IsActive = req.IsActive.Value;

            await _db.SaveChangesAsync(ct);

            return new PostDetailDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Type = post.Type,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                IsActive = post.IsActive,
                CreatedAt = post.CreatedAt
            };
        }

        // ----------------------------
        // DELETE POST
        // ----------------------------
        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(p => p.PostId == id, ct);
            if (post is null) return false;

            _db.Posts.Remove(post);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
