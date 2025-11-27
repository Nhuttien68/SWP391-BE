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
    public class PostPackgeService : IPostPackgeService
    {
        private readonly PostPackageRepository _postPackageRepository;
        private readonly TimeHelper _timHelpere ;
        public PostPackgeService(PostPackageRepository postPackageRepository, TimeHelper timHelpere)
        {
            _postPackageRepository = postPackageRepository;
            _timHelpere = timHelpere;
        }

        public async Task<BaseResponse> CreatePostPackageAsync(CreatePostPackageDTO createPostPackageDTO)
        {
            try
            {
                var vietNamtime = _timHelpere.GetVietNamTime();
                var newpostPackage = new PostPackage
                {
                    PackageId = Guid.NewGuid(),
                    PackageName = createPostPackageDTO.PackageName,
                    Price = createPostPackageDTO.Price,
                    DurationInDays = createPostPackageDTO.DurationInDays,
                    CreatedAt = vietNamtime,
                    IsActive = true
                };
                await _postPackageRepository.CreateAsync(newpostPackage);
                return new BaseResponse
                {
                    Status = StatusCodes.Status201Created.ToString(),
                    Message = "Post package created successfully."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"An error occurred while creating the post package: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse> DeletePostPackageAsync(Guid id)
        {
            try
            {
                var postPackage = await _postPackageRepository.GetByIdAsync(id);
                if (postPackage == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Post package not found."
                    };
                }
                postPackage.IsActive = false;
                await _postPackageRepository.UpdateAsync(postPackage);
                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Post package deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"An error occurred while deleting the post package: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<BaseResponse>> GetAllPostPackagesAsync()
        {
            try
            {
                var postPackages = await _postPackageRepository.GetAllPostPackageAsync();
                var response = postPackages.Select(pp => new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Post package retrieved successfully.",
                    Data = new PostPackgeDTOResponse
                    {
                        Id = pp.PackageId,
                        PackageName = pp.PackageName,
                        Price = pp.Price,
                        DurationInDays = pp.DurationInDays,
                        CreatedAt = pp.CreatedAt ?? DateTime.MinValue,
                        isActive = pp.IsActive
                    }
                });
                return response;
            }
            catch (Exception ex)
            {
                return new List<BaseResponse>
                {
                    new BaseResponse
                    {
                        Status = StatusCodes.Status500InternalServerError.ToString(),
                        Message = $"An error occurred while retrieving post packages: {ex.Message}"
                    }
                };
            }
        }

        public async Task<BaseResponse> GetPostPackageByIdAsync(Guid id)
        {
            try
            {
                var postPackage = await _postPackageRepository.GetByIdAsync(id);
                if (postPackage == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Post package not found."
                    };
                }
                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Post package retrieved successfully.",
                    Data = new PostPackgeDTOResponse
                    {
                        Id = postPackage.PackageId,
                        PackageName = postPackage.PackageName,
                        Price = postPackage.Price,
                        DurationInDays = postPackage.DurationInDays,
                        CreatedAt = postPackage.CreatedAt ?? DateTime.MinValue,
                        isActive = postPackage.IsActive
                    }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"An error occurred while retrieving the post package: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse> UpdatePostPackageAsync(UpdatePostPackageDTO updatePostPackageDTO)
        {
            var postPackage = await _postPackageRepository.GetByIdAsync(updatePostPackageDTO.Id);
            if (postPackage == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Post package not found."
                };
            }
            postPackage.PackageName = updatePostPackageDTO.PackageName;
            postPackage.Price = updatePostPackageDTO.Price;
            postPackage.DurationInDays = updatePostPackageDTO.DurationInDays;
            try
            {
                await _postPackageRepository.UpdateAsync(postPackage);
                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Post package updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"An error occurred while updating the post package: {ex.Message}"
                };
            }
        }
    }
}
