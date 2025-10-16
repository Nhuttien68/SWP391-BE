using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Interfaces
{
    public interface IVehicleBrandService
    {
        Task<BaseResponse> CreateVehicleBrandAsync(VehiCleBrandRequestDTO requestDTO);
        Task<BaseResponse> UpdateVehicleBrandAsync(VehiCleBrandUpdateRequestDTO requestDTO);
        Task<BaseResponse> DeleteVehicleBrandAsync(Guid brandId);
        Task<BaseResponse> GetAllVehicleBrandsAsync();
        Task<BaseResponse> GetVehicleBrandByIdAsync(Guid brandId);
    }
}
