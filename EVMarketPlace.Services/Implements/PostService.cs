using EVMarketPlace.Repositories.RequestDTO.Posts;
using EVMarketPlace.Repositories.ResponseDTO.Posts;
using EVMarketPlace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.Utils;

namespace EVMarketPlace.Services.Implements
{


    public class PostService : IPostService 
    {
        private readonly PostRepository _postRepository;
        private readonly UserUtility _userUtility;
        public PostService( PostRepository postRepository , UserUtility userUtility )
        {
            _postRepository = postRepository;
            _userUtility = userUtility;
        }
        // Tạo mới Post
        // CancellationToken để hủy tác vụ khi client ngắt request (ví dụ: đóng tab, timeout) giúp tiết kiệm tài nguyên.
        public async Task<PostDto> CreateAsync(PostCreateRequest req, CancellationToken ct = default) 

        {
            var userid = _userUtility.GetUserIdFromToken();
            if (userid == Guid.Empty)
            {
                throw new UnauthorizedAccessException("User ID does not found in token.");
            }
            var post = new Post
            {
                PostId = Guid.NewGuid(),
                UserId = userid,
                Type = req.Type,
                Title = req.Title,
                Description = req.Description,
                Price = req.Price,
                IsActive =  false,  
                CreatedAt = DateTime.UtcNow         // dùng UTC cho đồng nhất
            };

            await _postRepository.CreateAsync(post);
            return ToDto(post);
        }

        //Lấy 1 post theo Id(read-only)
        public async Task<PostDto?> GetByIdAsync(Guid postId, CancellationToken ct = default)
        {
            var post = await _postRepository.GetByIdAsync(postId);

            return post is null ? null : ToDto(post);
        }
        // getall
        public async Task<IReadOnlyList<PostDto>> GetAllAsync(CancellationToken ct = default)
        {
            var posts = await _postRepository.GetAllAsync();
            return posts.Select(p => ToDto(p)).ToList();
        }




        // Cập nhật Post (partial update: gửi gì sửa nấy)
        public async Task<PostDto> UpdateAsync(PostUpdateRequest req, CancellationToken ct = default)
        {
            var post = await _postRepository.GetByIdAsync(req.PostId);
            if (post == null)
                throw new KeyNotFoundException("Post không tồn tại");

            post.Type = req.Type ?? post.Type;
            post.Title = req.Title ?? post.Title;
            post.Description = req.Description ?? post.Description;
            post.Price = req.Price ?? post.Price;
            post.IsActive = req.IsActive ?? post.IsActive;

            await _postRepository.UpdateAsync(post);
            return ToDto(post);
        }




        //Xóa Post theo Id
        public async Task<bool> DeleteAsync(Guid postId, CancellationToken ct = default)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null) return false;

            await _postRepository.RemoveAsync(post);
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
