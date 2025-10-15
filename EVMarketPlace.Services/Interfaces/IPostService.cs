using EVMarketPlace.Repositories.RequestDTO.Posts;
using EVMarketPlace.Repositories.ResponseDTO.Posts;
using Microsoft.AspNetCore.Http;

namespace EVMarketPlace.Services.Interfaces
{

    // Hợp đồng cho CRUD Post. Controller chỉ gọi qua interface này.

    public interface IPostService
    {
        Task<PostDto> CreateAsync(PostCreateRequest req,IFormFile? image, CancellationToken ct = default);
        Task<PostDto?> GetByIdAsync(Guid postId, CancellationToken ct = default);
        Task<IReadOnlyList<PostDto>> GetAllAsync(CancellationToken ct = default);
        Task<PostDto> UpdateAsync(PostUpdateRequest req, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid postId, CancellationToken ct = default);
    }
}
