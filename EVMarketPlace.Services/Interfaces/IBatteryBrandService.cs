using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IBatteryBrandService
    {
        Task<BaseResponse> CreateBatteryAsync(BatteryBrandRequestDTO requestDTO);
        Task<BaseResponse> UpdateBatteryAsync(UpdateBatteryBrandRequestDTO requestDTO);
        Task<BaseResponse> DeleteBatteryAsync(Guid BrandBatteryId);
        Task<BaseResponse> GetAllBatteryAsync();
        Task<BaseResponse> GetBatteryByIdAsync(Guid BrandBatteryId);

    }
}
