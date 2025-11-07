using EVMarketPlace.Repositories.Enum;
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
        Task<BaseResponse> GetAllPostWithPendding();
        Task<BaseResponse> ApprovedStatus(Guid PostId);
        Task<BaseResponse> RejectStatusAsync(Guid PostId);
        Task<BaseResponse> CountPostsByStatusAsync(PostStatusEnum status);
        // Lọc theo ngày + status
        Task<BaseResponse> GetPostsByDateAndStatusAsync( int day,int month,int year,PostStatusEnum status);

        // Lọc theo tháng + status
        Task<BaseResponse> GetPostsByMonthAndStatusAsync(int month,int year, PostStatusEnum status);

        // Lọc theo năm + status
        Task<BaseResponse> GetPostsByYearAndStatusAsync(int year, PostStatusEnum status);
    }
}
