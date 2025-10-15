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
        Task<BaseRespone> CreateBatteryAsync(BatteryBrandRequestDTO requestDTO);
        Task<BaseRespone> UpdateBatteryAsync(UpdateBatteryBrandRequestDTO requestDTO);
        Task<BaseRespone> DeleteBatteryAsync(Guid BrandBatteryId);
        Task<BaseRespone> GetAllBatteryAsync();
        Task<BaseRespone> GetBatteryByIdAsync(Guid BrandBatteryId);

    }
}
