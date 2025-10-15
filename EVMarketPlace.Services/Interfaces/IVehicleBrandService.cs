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
        Task<BaseRespone> CreateVehicleBrandAsync(VehiCleBrandRequestDTO requestDTO);
        Task<BaseRespone> UpdateVehicleBrandAsync(VehiCleBrandUpdateRequestDTO requestDTO);
        Task<BaseRespone> DeleteVehicleBrandAsync(Guid brandId);
        Task<BaseRespone> GetAllVehicleBrandsAsync();
        Task<BaseRespone> GetVehicleBrandByIdAsync(Guid brandId);
    }
}
