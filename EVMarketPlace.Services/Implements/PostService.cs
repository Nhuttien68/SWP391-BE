using EVMarketPlace.Repositories.RequestDTO.Posts;
using EVMarketPlace.Repositories.ResponseDTO.Posts;
using EVMarketPlace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Repository;

namespace EVMarketPlace.Services.Implements
{


    public class PostService : IPostService 
    {
        // private readonly EvMarketplaceContext _db;
        private readonly PostRepository _postRepository;
        // public PostService(EvMarketplaceContext db) => _db = db;
        public PostService( PostRepository postRepository)
        {
            _postRepository = postRepository;
        }
        // Tạo mới Post
        // CancellationToken để hủy tác vụ khi client ngắt request (ví dụ: đóng tab, timeout) giúp tiết kiệm tài nguyên.
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
                IsActive =  false,  
                CreatedAt = DateTime.UtcNow         // dùng UTC cho đồng nhất
            };

            await _postRepository.CreateAsync(post);
            //await _db.Set<Post>().AddAsync(post, ct);
            //await _db.SaveChangesAsync(ct);

            return ToDto(post);
        }

        //Lấy 1 post theo Id(read-only)
        public async Task<PostDto?> GetByIdAsync(Guid postId, CancellationToken ct = default)
        {
            //var post = await _db.Set<Post>()
            //                 .AsNoTracking()
            //                 .FirstOrDefaultAsync(x => x.PostId == postId, ct);
            var post = await _postRepository.GetByIdAsync(postId);

            return post is null ? null : ToDto(post);
        }

        //// Lấy danh sách Post (phân trang cơ bản + filter nhẹ)
        //public async Task<IReadOnlyList<PostDto>> GetPagedAsync(
        //    int page = 1, int pageSize = 10, string? keyword = null, string? type = null, CancellationToken ct = default)
        //{
        //    // Giới hạn phân trang
        //    page = Math.Max(page, 1);
        //    pageSize = Math.Max(pageSize, 10);

        //    var query = _db.Set<Post>().AsNoTracking(); // cái dòng này làm gì á ông ? chat gpt :)) vl 

        //    // Lọc theo từ khóa
        //    if (!string.IsNullOrWhiteSpace(keyword))
        //        query = query.Where(p => p.Title.Contains(keyword) || p.Description.Contains(keyword));

        //    // Lọc theo loại post
        //    if (!string.IsNullOrWhiteSpace(type))
        //        query = query.Where(p => p.Type == type);

        //    // Lấy danh sách
        //    var posts = await query
        //        .OrderByDescending(p => p.CreatedAt)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync(ct);

        //    return posts.Select(ToDto).ToList();
        //}


        // Cập nhật Post (partial update: gửi gì sửa nấy)
        public async Task<PostDto> UpdateAsync(PostUpdateRequest req, CancellationToken ct = default)
        {
            //var post = await _db.Set<Post>().FirstOrDefaultAsync(p => p.PostId == req.PostId, ct);
            var post = await _postRepository.GetByIdAsync(req.PostId);
            if (post == null)
                throw new KeyNotFoundException("Post không tồn tại");

            post.Type = req.Type ?? post.Type;
            post.Title = req.Title ?? post.Title;
            post.Description = req.Description ?? post.Description;
            post.Price = req.Price ?? post.Price;
            post.IsActive = req.IsActive ?? post.IsActive;

            //_db.Entry(post).State = EntityState.Modified; // bắt EF ghi UPDATE chắc chắn
            //await _db.SaveChangesAsync(ct);
            await _postRepository.UpdateAsync(post);
            return ToDto(post);
        }




        //Xóa Post theo Id
        public async Task<bool> DeleteAsync(Guid postId, CancellationToken ct = default)
        {
           // var post = await _db.Set<Post>().FirstOrDefaultAsync(p => p.PostId == postId, ct);
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null) return false;

            await _postRepository.RemoveAsync(post);
            //_db.Set<Post>().Remove(post);
            //await _db.SaveChangesAsync(ct);
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
