using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO.Posts;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.ResponseDTO.Posts;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace EVMarketPlace.Services.Implements
{


    public class PostService : IPostService
    {
        private readonly PostRepository _postRepository;
        private readonly UserUtility _userUtility;
        private readonly WalletRepository _walletRepository;
        private readonly FirebaseStorageService _firebaseStorage;
        
        public PostService(WalletRepository walletRepositpry, PostRepository postRepository , UserUtility userUtility, FirebaseStorageService firebaseStorage, PostImageRepository postImageRepository)
        {
            _postRepository = postRepository;
            _userUtility = userUtility;
            _firebaseStorage = firebaseStorage;
            _walletRepository = walletRepositpry;
        }

        public async Task<BaseResponse> ApprovedStatus(Guid id)
        {
            var adminUserId = _userUtility.GetUserIdFromToken();
            if (adminUserId == Guid.Empty)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status401Unauthorized.ToString(),
                    Message = "Unauthorized: admin user id not found"
                };
            }
            var adminWallet = await _walletRepository.GetWalletByUserIdAsync(adminUserId);
            if (adminWallet == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "Admin wallet not found"
                };
            }

            var walletResult = await _walletRepository.TryUpdateBalanceAsync(adminWallet.WalletId, 100000);
            if (!walletResult.Success)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "Failed to credit admin wallet"
                };
            }
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null) {
                return new BaseResponse
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "post not found"
                };
            }
            post.Status = PostStatusEnum.APPROVED.ToString();
            await _postRepository.UpdateAsync(post);
            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "change status post success",
            };
            
        }
        // Tạo bài đăng về pin
        public async Task<BaseResponse> CreateBatteryPostAsync(PostCreateBatteryRequest request)
        {
            try
            {
                var userId = _userUtility.GetUserIdFromToken();
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in token.");
                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                    throw new Exception("Wallet not found for the user.");
                var walletResult = await _walletRepository.TryUpdateBalanceAsync(wallet.WalletId, -100000);

                if (!walletResult.Success)
                {
                    return new BaseResponse
                    {
                        Status = "400",
                        Message = "Số dư ví không đủ để đăng bài.",
                        Data = null
                    };
                }
                var newPost = new Post
                {
                    PostId = Guid.NewGuid(),
                    UserId = userId,
                    Type = PostTypeEnum.BATTERY.ToString(),
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    CreatedAt = DateTime.UtcNow,
                    Status = PostStatusEnum.PENNDING.ToString(),
                    PostImages = new List<PostImage>(),
                    Battery = new Battery
                    {
                        BatteryId = Guid.NewGuid(),
                        BrandId = request.batteryCreateDto.BranId,
                        Capacity = request.batteryCreateDto.Capacity,
                        Condition = request.batteryCreateDto.Condition
                    }
                };

                // ✅ Upload ảnh
                if (request.Images != null && request.Images.Count > 0)
                {
                    foreach (var image in request.Images)
                    {
                        string? imageUrl = null;
                        if (image != null)
                        {
                            using var stream = image.OpenReadStream();
                            imageUrl = await _firebaseStorage.UploadFileAsync(stream, image.FileName, image.ContentType);
                        }

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            var postImage = new PostImage
                            {
                                ImageId = Guid.NewGuid(),
                                PostId = newPost.PostId,
                                ImageUrl = imageUrl,
                                UploadedAt = DateTime.Now,
                            };
                            newPost.PostImages.Add(postImage);
                        }
                    }
                }
                await _postRepository.CreateAsync(newPost);
                // ✅ Map sang DTO
                var postDto = new PostResponseDto
                {
                    PostId = newPost.PostId,
                    Title = newPost.Title,
                    Description = newPost.Description,
                    Price = newPost.Price,
                    Type = newPost.Type,
                    CreatedAt = newPost.CreatedAt,
                    Status = newPost.Status,
                    ImageUrls = newPost.PostImages.Select(i => i.ImageUrl).ToList(),
                };
                return new BaseResponse
                {
                    Status = StatusCodes.Status201Created.ToString(),
                    Message = "Post created successfully.",
                    Data = postDto
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        // Tạo bài đăng về xe
        public async Task<BaseResponse> CreateVehiclePostAsync(PostCreateVehicleRequest request)
        {
            try
            {
                
                var userId =  _userUtility.GetUserIdFromToken();
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in token.");
                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                    throw new Exception("Wallet not found for the user.");
                var walletResult = await _walletRepository.TryUpdateBalanceAsync(wallet.WalletId, -100000);

                if (!walletResult.Success)
                {
                    return new BaseResponse
                    {
                        Status = "400",
                        Message = "Số dư ví không đủ để đăng bài.",
                        Data = null
                    };
                }
                var newPost = new Post
                {
                    PostId = Guid.NewGuid(),
                    UserId = userId,
                    Type = PostTypeEnum.VEHICLE.ToString(),
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    CreatedAt = DateTime.UtcNow,
                    Status = PostStatusEnum.PENNDING.ToString(),

                    PostImages = new List<PostImage>(),
                    Vehicle = new Vehicle
                    {
                        VehicleId = Guid.NewGuid(),
                        BrandId = request.vehicleCreateDto.BrandId,
                        Model = request.vehicleCreateDto.Model,
                        Year = request.vehicleCreateDto.Year,
                        Mileage = request.vehicleCreateDto.Mileage
                    }
                };

                // ✅ Upload ảnh
                if (request.Images != null && request.Images.Count > 0)
                {
                    foreach (var image in request.Images)
                    {
                        string? imageUrl = null;
                        if (image != null)
                        {
                            using var stream = image.OpenReadStream();
                            imageUrl = await _firebaseStorage.UploadFileAsync(stream, image.FileName, image.ContentType);
                        }

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            var postImage = new PostImage
                            {
                                ImageId = Guid.NewGuid(),
                                PostId = newPost.PostId,
                                ImageUrl = imageUrl,
                                UploadedAt = DateTime.Now,
                            };
                            newPost.PostImages.Add(postImage);
                        }
                    }
                }

                await _postRepository.CreateAsync(newPost);
               

                // ✅ Map sang DTO
                var postDto = new PostResponseDto
                {
                    PostId = newPost.PostId,
                    Title = newPost.Title,
                    Description = newPost.Description,
                    Price = newPost.Price,
                    Type = newPost.Type,
                    CreatedAt = newPost.CreatedAt,
                    Status = newPost.Status,
                    ImageUrls = newPost.PostImages.Select(i => i.ImageUrl).ToList()
                };

                return new BaseResponse
                {
                    Status = StatusCodes.Status201Created.ToString(),
                    Message = "Post created successfully.",
                    Data = postDto
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        // Xóa bài đăng (soft delete)
        public async Task<BaseResponse> DeletePostAsync(Guid postId)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(postId);
                if (post == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Post not found."
                    };
                }
                post.Status = PostStatusEnum.DELETED.ToString();

                await _postRepository.UpdateAsync(post);

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Post deleted (soft) successfully."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        // Lấy tất cả bài đăng
        public async Task<BaseResponse> GetAllPostsAsync()
        {
            var posts = await _postRepository.GetAllPostWithImageAsync();

            var response = posts.Select(p => new PostResponseDto
            {
                PostId = p.PostId,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Type = p.Type,
                CreatedAt = p.CreatedAt,
                Status = p.Status,
                ImgId = p.PostImages?.Select(i => i.ImageId).ToList(),
                ImageUrls = p.PostImages?.Select(i => i.ImageUrl).ToList(),

                //  Nếu là bài đăng về xe
                Vehicle = p.Vehicle != null ? new VehicleDto
                {
                    VehicleId = p.Vehicle.VehicleId,
                    BrandName = p.Vehicle.Brand?.Name ?? "Unknown",
                    Model = p.Vehicle.Model ?? "",
                    Year = p.Vehicle.Year,
                    Mileage = p.Vehicle.Mileage
                } : null,

                //  Nếu là bài đăng về pin
                Battery = p.Battery != null ? new BatteryDto
                {
                    BatteryId = p.Battery.BatteryId,
                    BrandName = p.Battery.Brand?.Name ?? "Unknown",
                    Capacity = p.Battery.Capacity,
                    Condition = p.Battery.Condition ?? ""
                } : null,
                User = new UserinformationResponse
                {
                    UserId = p.User.UserId,
                    FullName = p.User.FullName,
                    Email = p.User.Email,
                    Phone = p.User.Phone,
                    Status = p.User.Status,
                    Role = p.User.Role
                }
            }).ToList();

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Get all posts successfully.",
                Data = response
            };
        }
        // Lấy tất cả bài đăng đang chờ duyệt
        public async Task<BaseResponse> GetAllPostWithPendding()
        {
            var posts = await _postRepository.GetAllPostWithPennding();
            var response = posts.Select(p => new PostResponseDto
            {
                PostId = p.PostId,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Type = p.Type,
                CreatedAt = p.CreatedAt,
                Status = p.Status,
                ImgId = p.PostImages?.Select(i => i.ImageId).ToList(),
                ImageUrls = p.PostImages?.Select(i => i.ImageUrl).ToList(),

                //  Nếu là bài đăng về xe
                Vehicle = p.Vehicle != null ? new VehicleDto
                {
                    VehicleId = p.Vehicle.VehicleId,
                    BrandName = p.Vehicle.Brand?.Name ?? "Unknown",
                    Model = p.Vehicle.Model ?? "",
                    Year = p.Vehicle.Year,
                    Mileage = p.Vehicle.Mileage
                } : null,

                //  Nếu là bài đăng về pin
                Battery = p.Battery != null ? new BatteryDto
                {
                    BatteryId = p.Battery.BatteryId,
                    BrandName = p.Battery.Brand?.Name ?? "Unknown",
                    Capacity = p.Battery.Capacity,
                    Condition = p.Battery.Condition ?? ""
                } : null,
                User = new UserinformationResponse
                {
                    UserId = p.User.UserId,
                    FullName = p.User.FullName,
                    Email = p.User.Email,
                    Phone = p.User.Phone,
                    Status = p.User.Status,
                    Role = p.User.Role
                }
            }).ToList();
            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Get all posts successfully.",
                Data = response
            };
        }
        // Lấy bài đăng theo Id
        public async Task<BaseResponse> GetPostByIdAsync(Guid postId)
        {
            var post = await _postRepository.GetPostByIdWithImageAsync(postId);

            if (post == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Post not found."
                };
            }

            var postDto = new PostResponseDto
            {
                PostId = post.PostId,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                Type = post.Type,
                CreatedAt = post.CreatedAt,
                Status = post.Status,
                ImgId = post.PostImages?.Select(i => i.ImageId).ToList(),
                ImageUrls = post.PostImages?.Select(i => i.ImageUrl).ToList(),
                User = new UserinformationResponse
                {
                    UserId = post.User.UserId,
                    FullName = post.User.FullName,
                    Email = post.User.Email,
                    Phone = post.User.Phone,
                    Status = post.User.Status,
                    Role = post.User.Role
                }

            };

            // ✅ Map Vehicle (1-1)
            if (post.Vehicle != null)
            {
                postDto.Vehicle = new VehicleDto
                {
                    VehicleId = post.Vehicle.VehicleId,
                    BrandName = post.Vehicle.Brand?.Name ?? "Unknown",
                    Model = post.Vehicle.Model ?? "",
                    Year = post.Vehicle.Year,
                    Mileage = post.Vehicle.Mileage
                };
            }

            // ✅ Map Battery (1-1)
            if (post.Battery != null)
            {
                postDto.Battery = new BatteryDto
                {
                    BatteryId = post.Battery.BatteryId,
                    BrandName = post.Battery.Brand?.Name ?? "Unknown",
                    Capacity = post.Battery.Capacity,
                    Condition = post.Battery.Condition ?? ""
                };
            }

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Get post successfully.",
                Data = postDto
            };
        }
        // Lấy bài đăng theo UserId
        public async Task<BaseResponse> GetPostByUserIdAsync()
        {
            try
            {
                var userId = _userUtility.GetUserIdFromToken();
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in token.");
                var posts = await _postRepository.GetPostsByUserIdAsync(userId);
                var response = posts.Select(p => new PostResponseDto
                {
                    PostId = p.PostId,
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Type = p.Type,
                    CreatedAt = p.CreatedAt,
                    Status = p.Status,
                    ImgId = p.PostImages?.Select(i => i.ImageId).ToList(),
                    ImageUrls = p.PostImages?.Select(i => i.ImageUrl).ToList(),

                    //  Nếu là bài đăng về xe
                    Vehicle = p.Vehicle != null ? new VehicleDto
                    {
                        VehicleId = p.Vehicle.VehicleId,
                        BrandName = p.Vehicle.Brand?.Name ?? "Unknown",
                        Model = p.Vehicle.Model ?? "",
                        Year = p.Vehicle.Year,
                        Mileage = p.Vehicle.Mileage
                    } : null,

                    //  Nếu là bài đăng về pin
                    Battery = p.Battery != null ? new BatteryDto
                    {
                        BatteryId = p.Battery.BatteryId,
                        BrandName = p.Battery.Brand?.Name ?? "Unknown",
                        Capacity = p.Battery.Capacity,
                        Condition = p.Battery.Condition ?? ""
                    } : null,
                    User = new UserinformationResponse
                    {
                        UserId = p.User.UserId,
                        FullName = p.User.FullName,
                        Email = p.User.Email,
                        Phone = p.User.Phone,
                        Status = p.User.Status,
                        Role = p.User.Role
                    }
                }).ToList();


                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Get post successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        // Từ chối bài đăng và hoàn tiền
        public async Task<BaseResponse> RejectStatusAsync(Guid PostId)
        {
            try
            {
                var post = await _postRepository.GetByIdAsync(PostId);
                if (post == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Post not found."
                    };
                }
                post.Status = PostStatusEnum.REJECTED.ToString();
                await _postRepository.UpdateAsync(post);
                if (post.UserId == null || post.UserId == Guid.Empty)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status500InternalServerError.ToString(),
                        Message = "UserId of post is invalid."
                    };
                }

                var userWallet = await _walletRepository.GetWalletByUserIdAsync(post.UserId.Value);
                if (userWallet == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status500InternalServerError.ToString(),
                        Message = "User wallet not found."
                    };
                }

                var walletResult = await _walletRepository.TryUpdateBalanceAsync(userWallet.WalletId, 100000m);
                if (!walletResult.Success)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status500InternalServerError.ToString(),
                        Message = "Failed to refund user wallet."
                    };
                }

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Post rejected and 100,000 refunded to user.",
                    Data = new { UserNewBalance = walletResult.NewBalance }
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Error: {ex.Message}"
                };
            }
        }
        // Cập nhật bài đăng về pin
        public async Task<BaseResponse> UpdateBatteryPostAsync(UpdateBatteryPostRequest request)
        {
            try
            {
                var post = await _postRepository.GetPostByIdWithImageAsync(request.PostId);
                if (post == null)
                    return new BaseResponse { Status = StatusCodes.Status404NotFound.ToString(), Message = "Post not found" };

                
                post.Title = request.Title ?? post.Title;
                post.Description = request.Description ?? post.Description;
                post.Price = request.Price ?? post.Price;

                
                if (post.Battery != null)
                {
                    post.Battery.BrandId = request.BrandId ?? post.Battery.BrandId;
                    post.Battery.Capacity = request.Capacity ?? post.Battery.Capacity;
                    post.Battery.Condition = request.Condition ?? post.Battery.Condition;
                }

                await UpdatePostImagesAsync(post, request.KeepImageIds, request.NewImages);

                await _postRepository.UpdateBatteryAsync(post);
                return new BaseResponse
                {
                    Status = StatusCodes.Status201Created.ToString(),
                    Message = "Battery post updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Status = StatusCodes.Status500InternalServerError.ToString(), Message = ex.Message };
            }
        }
        // Cập nhật bài đăng về xe
        public async Task<BaseResponse> UpdateVehiclePostAsync(UpdateVehiclePostRequest request)
        {
            try
            {
                var post = await _postRepository.GetPostByIdWithImageAsync(request.PostId);
                if (post == null)
                    return new BaseResponse { Status = StatusCodes.Status404NotFound.ToString(), Message = "Post not found" };

              
                post.Title = request.Title ?? post.Title;
                post.Description = request.Description ?? post.Description;
                post.Price = request.Price ?? post.Price;

               
                if (post.Vehicle != null)
                {
                    post.Vehicle.BrandId = request.BrandId ?? post.Vehicle.BrandId;
                    post.Vehicle.Model = request.Model ?? post.Vehicle.Model;
                    post.Vehicle.Year = request.Year ?? post.Vehicle.Year;
                    post.Vehicle.Mileage = request.Mileage ?? post.Vehicle.Mileage;
                }

               
                await UpdatePostImagesAsync(post, request.KeepImageIds, request.NewImages);

                await _postRepository.UpdateVehicleAsync(post);
                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Vehicle post updated successfully."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse { Status =StatusCodes.Status500InternalServerError.ToString(), Message = ex.Message };
            }
        }
        // Cập nhật hình ảnh bài đăng
        private async Task UpdatePostImagesAsync(Post post, List<Guid>? keepImageIds, List<IFormFile>? newImages)
        {
           
            if (keepImageIds != null && post.PostImages.Any())
            {
                var toDelete = post.PostImages.Where(img => !keepImageIds.Contains(img.ImageId)).ToList();
                foreach (var img in toDelete)
                {
                    await _firebaseStorage.DeleteFileAsync(img.ImageUrl);
                    post.PostImages.Remove(img); 
                }
            }

           
            if (newImages != null && newImages.Count > 0)
            {
                foreach (var image in newImages)
                {
                    using var stream = image.OpenReadStream();
                    var imageUrl = await _firebaseStorage.UploadFileAsync(stream, image.FileName, image.ContentType);

                    post.PostImages.Add(new PostImage
                    {
                        ImageId = Guid.NewGuid(),
                        PostId = post.PostId,
                        ImageUrl = imageUrl,
                        UploadedAt = DateTime.Now
                    });
                }
            }
        }
    }
}
