using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using System.Security.Claims;

namespace EVMarketPlace.Services.Interfaces
{
    public interface ICartService
    {
        Task<BaseResponse> AddToCartAsync(ClaimsPrincipal user, AddToCartRequest request);
        Task<BaseResponse> GetCartByUserAsync(ClaimsPrincipal user);
        Task<BaseResponse> UpdateCartItemAsync(ClaimsPrincipal user, UpdateCartItemRequest request);
        Task<BaseResponse> RemoveFromCartAsync(ClaimsPrincipal user, RemoveFromCartRequest request);
        Task<BaseResponse> ClearCartAsync(ClaimsPrincipal user);
    }
}