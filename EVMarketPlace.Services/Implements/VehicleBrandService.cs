using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{
    public class VehicleBrandService : IVehicleBrandService
    {
        private readonly VehilceBrandRepository _vehicleBrandRepository;
        public VehicleBrandService(VehilceBrandRepository vehicleBrandRepository)
        {
            _vehicleBrandRepository = vehicleBrandRepository;
        }

        public async Task<BaseResponse> CreateVehicleBrandAsync(VehiCleBrandRequestDTO requestDTO)
        {
            try
            {
                var newbrand = new VehicleBrand
                {
                    BrandId = Guid.NewGuid(),
                    Name = requestDTO.BrandName,
                };
                await _vehicleBrandRepository.CreateAsync(newbrand);
                var response = new VehicleBrandResponseDTO
                {
                    BrandId = newbrand.BrandId,
                    BrandName = newbrand.Name
                };
                return new BaseResponse
                {
                    Status = StatusCodes.Status201Created.ToString(),
                    Message = "Create vehicle brand successfully",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "An error occurred while creating the vehicle brand.",
                    Data = ex.Message
                };

            }
        }
        public async Task<BaseResponse> DeleteVehicleBrandAsync(Guid brandId)
        {
            var brand = await _vehicleBrandRepository.GetByIdAsync(brandId);
            if (brand == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Vehicle brand not found.",

                };
            }
            await _vehicleBrandRepository.RemoveAsync(brand);
            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Vehicle brand deleted successfully.",

            };

        }

        public async Task<BaseResponse> GetAllVehicleBrandsAsync()
        {
            try
            {
                var brands = await _vehicleBrandRepository.GetAllAsync();
                var brandDTOs = brands.Select(b => new VehicleBrandResponseDTO
                {
                    BrandId = b.BrandId,
                    BrandName = b.Name
                }).ToList();
                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Vehicle brands retrieved successfully.",
                    Data = brandDTOs
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "An error occurred while retrieving vehicle brands.",
                    Data = ex.Message
                };
            }

        }

        public async Task<BaseResponse> GetVehicleBrandByIdAsync(Guid brandId)
        {
            try
            {
                var brand = await _vehicleBrandRepository.GetByIdAsync(brandId);
                if (brand == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Vehicle brand not found.",

                    };
                }
                var brandDTO = new VehicleBrandResponseDTO
                {
                    BrandId = brand.BrandId,
                    BrandName = brand.Name
                };
                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Vehicle brand retrieved successfully.",
                    Data = brandDTO
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "An error occurred while retrieving the vehicle brand.",
                    Data = ex.Message
                };
            }

        }

        public async Task<BaseResponse> UpdateVehicleBrandAsync(VehiCleBrandUpdateRequestDTO requestDTO)
        {
            try
            {
                var brand = await _vehicleBrandRepository.GetByIdAsync(requestDTO.BrandId);
                if (brand == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Vehicle brand not found.",

                    };
                }
                brand.Name = requestDTO.BrandName;
                await _vehicleBrandRepository.UpdateAsync(brand);
                var brandDTO = new VehicleBrandResponseDTO
                {
                    BrandId = brand.BrandId,
                    BrandName = brand.Name
                };
                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Vehicle brand updated successfully.",
                    Data = brandDTO
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "An error occurred while updating the vehicle brand.",
                    Data = ex.Message
                };

            }
        }
    }
}
