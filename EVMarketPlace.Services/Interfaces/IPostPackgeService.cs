using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IPostPackgeService
    {
        Task<BaseResponse> CreatePostPackageAsync(CreatePostPackageDTO createPostPackageDTO);
        Task<BaseResponse> GetPostPackageByIdAsync(Guid id);
        Task<IEnumerable<BaseResponse>> GetAllPostPackagesAsync();
        Task<BaseResponse> UpdatePostPackageAsync(UpdatePostPackageDTO updatePostPackageDTO);
        Task<BaseResponse> DeletePostPackageAsync(Guid id);
    }
}
