using EVMarketPlace.Repositories.RequestDTO.Posts;
using EVMarketPlace.Repositories.ResponseDTO.Posts;

namespace EVMarketPlace.Services.Interfaces
{

    // Hợp đồng cho CRUD Post. Controller chỉ gọi qua interface này.

    public interface IPostService
    {
        Task<PostDto> CreateAsync(PostCreateRequest req, CancellationToken ct = default);
        Task<PostDto?> GetByIdAsync(Guid postId, CancellationToken ct = default);

        // Lấy danh sách (có phân trang nhẹ).
        Task<IReadOnlyList<PostDto>> GetPagedAsync(
            int page = 1,
            int pageSize = 10,
            string? keyword = null,
            string? type = null,
            CancellationToken ct = default);

        Task<PostDto> UpdateAsync(PostUpdateRequest req, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid postId, CancellationToken ct = default);
    }
}
