using EVMarketPlace.Repositories.RequestDTO.Posts;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.ResponseDTO.Posts;
using Microsoft.AspNetCore.Http;

namespace EVMarketPlace.Services.Interfaces
{

    // Hợp đồng cho CRUD Post. Controller chỉ gọi qua interface này.

    public interface IPostService
    {
        Task<BaseResponse> CreateVehiclePostAsync(PostCreateVehicleRequest request);
        Task<BaseResponse> CreateBatteryPostAsync(PostCreateBatteryRequest request);
        Task<BaseResponse> GetAllPostsAsync();
        Task<BaseResponse> GetPostByIdAsync(Guid postId);
        Task<BaseResponse> GetPostByUserIdAsync();
        Task<BaseResponse> UpdateVehiclePostAsync(UpdateVehiclePostRequest request);
        Task<BaseResponse> UpdateBatteryPostAsync(UpdateBatteryPostRequest request);
        Task<BaseResponse> DeletePostAsync(Guid postId);

      
    }
}
