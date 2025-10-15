using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{
    public class BatteryBrandService : IBatteryBrandService
    {
        private readonly BatteryBrandRepository _batteryBrandRepository;
       
        public BatteryBrandService(BatteryBrandRepository batteryBrandRepository, UserUtility userUtility)
        {
           
            _batteryBrandRepository = batteryBrandRepository;
        }
        public async Task<BaseRespone> CreateBatteryAsync(BatteryBrandRequestDTO requestDTO)
        {
           
            try
            {
                var newBrand = new BatteryBrand
                {
                    BrandId = Guid.NewGuid(),
                    Name = requestDTO.BrandName,

                };
                
                await _batteryBrandRepository.CreateAsync(newBrand);

                var responseDTO = new BatteryBrandResponseDTO
                {
                    BrandId = newBrand.BrandId,
                    BrandName = newBrand.Name
                };

                
                return new BaseRespone
                {
                    Status = StatusCodes.Status201Created.ToString(),
                    Message = "Battery brand created successfully.",
                    Data = responseDTO
                };
            }
            catch (Exception ex)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "An error occurred while creating the battery brand: " + ex.Message,
                    
                };

            }
        }

        public async Task<BaseRespone> DeleteBatteryAsync(Guid BrandBatteryId)
        {
           
            var existingBrand = await _batteryBrandRepository.GetByIdAsync(BrandBatteryId);
            if (existingBrand == null)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Battery brand not found",
                };
            }
            try
            {
                await _batteryBrandRepository.RemoveAsync(existingBrand);
                return new BaseRespone
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Battery brand deleted successfully",
                };
            }
            catch (Exception ex)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "An error occurred while deleting the battery brand: " + ex.Message,
                };
            }


        }

        public async Task<BaseRespone> GetAllBatteryAsync()
        {

            var brands = _batteryBrandRepository.GetAll();
            var brandList = brands.Select(b => new BatteryBrandResponseDTO
            {
                BrandId = b.BrandId,
                BrandName = b.Name
            }).ToList();
            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Battery brands retrieved successfully",
                Data = brandList
            };
        }

        public async Task<BaseRespone> GetBatteryByIdAsync(Guid BrandBatteryId)
        {
            
            var existingBrand = await _batteryBrandRepository.GetByIdAsync(BrandBatteryId);
            if (existingBrand == null)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Battery brand not found",

                };
            }
            var responseDTO = new BatteryBrandResponseDTO
            {
                BrandId = existingBrand.BrandId,
                BrandName = existingBrand.Name
            };
            return new BaseRespone
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Battery brand retrieved successfully",
                Data = responseDTO
            };
        }

        public async Task<BaseRespone> UpdateBatteryAsync(UpdateBatteryBrandRequestDTO requestDTO)
        {
           
            var existingBrand = await _batteryBrandRepository.GetByIdAsync(requestDTO.BatteryBrandId);
            if (existingBrand == null)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Battery brand not found",

                };
            }
            try
            {
                existingBrand.Name = requestDTO.BrandName;
                await _batteryBrandRepository.UpdateAsync(existingBrand);
                return new BaseRespone
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Battery brand updated successfully",
                    Data = existingBrand
                };
            }
            catch (Exception ex)
            {
                return new BaseRespone
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "An error occurred while updating the battery brand: " + ex.Message,

                };
            }
        }
    }
}
