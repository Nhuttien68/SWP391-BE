using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IPostService
    {
        Task<(int total, IEnumerable<PostListItemDto> items)> GetListAsync(string? keyword, string? type, int page, int pageSize, CancellationToken ct = default);
        Task<PostDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PostDetailDto> CreateAsync(CreatePostRequest req, CancellationToken ct = default);
        Task<PostDetailDto?> UpdateAsync(Guid id, UpdatePostRequest req, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
