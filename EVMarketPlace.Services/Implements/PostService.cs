using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
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
        private readonly PostPackageRepository _postPackageRepository;
        private readonly WalletTransactionRepository _walletTransactionRepository;

        public PostService(WalletTransactionRepository walletTransactionRepository, WalletRepository walletRepositpry, PostRepository postRepository , UserUtility userUtility, FirebaseStorageService firebaseStorage, PostImageRepository postImageRepository, PostPackageRepository postPackageRepository)
        {
            _postRepository = postRepository;
            _userUtility = userUtility;
            _firebaseStorage = firebaseStorage;
            _walletRepository = walletRepositpry;
            _postPackageRepository = postPackageRepository;
            _walletTransactionRepository = walletTransactionRepository;
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

            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status404NotFound.ToString(),
                    Message = "Post not found"
                };
            }

            // Lấy thông tin gói của bài đăng
            var postPackage = await _postPackageRepository.GetByIdAsync(post.PackageId);
            if (postPackage == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "Post package not found"
                };
            }

            // Admin nhận tiền phí duyệt theo giá của postPackage
            var adminWallet = await _walletRepository.GetWalletByUserIdAsync(adminUserId);
            if (adminWallet == null)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "Admin wallet not found"
                };
            }

            var walletResult = await _walletRepository.TryUpdateBalanceAsync(adminWallet.WalletId, postPackage.Price);
            if (!walletResult.Success)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = "Failed to credit admin wallet"
                };
            }

            // Tạo giao dịch WalletTransaction cho admin
            var transaction = new WalletTransaction
            {
                WalletTransactionId = Guid.NewGuid(),
                WalletId = adminWallet.WalletId,
                TransactionType = "POST_APPROVAL_FEE",
                Amount = postPackage.Price,
                BalanceBefore = adminWallet.Balance,
                BalanceAfter = adminWallet.Balance + postPackage.Price,
                Description = $"Nhận phí duyệt bài ({postPackage.PackageName})",
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.CreateAsync(transaction);

            // Cập nhật trạng thái bài đăng và thời gian hết hạn
            post.Status = PostStatusEnum.APPROVED.ToString();
            post.ExpireAt = post.CreatedAt.Value.AddDays(postPackage.DurationInDays);

            await _postRepository.UpdateAsync(post);

            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = "Post approved successfully. ExpireAt has been set.",
                Data = new { post.PostId, post.ExpireAt }
            };
        }



        public async Task<BaseResponse> CountPostsByStatusAsync(PostStatusEnum status)
        {
            try
            {
                var count = await _postRepository.CountPostsByStatusAsync(status);

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = $"Số lượng bài đăng có trạng thái {status}: {count}",
                    Data = new { Status = status.ToString(), Count = count }
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

        // Tạo bài đăng về pin
        public async Task<BaseResponse> CreateBatteryPostAsync(PostCreateBatteryRequest request)
        {
            try
            {
                var userId = _userUtility.GetUserIdFromToken();
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in token.");

                // Lấy ví người dùng
                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                    throw new Exception("Wallet not found for the user.");

                // Lấy thông tin gói đăng bài
                var postPackage = await _postPackageRepository.GetByIdAsync(request.postPackgeID);
                if (postPackage == null || !postPackage.IsActive.GetValueOrDefault())
                    throw new Exception("Package not found or inactive.");

                // Kiểm tra số dư
                if (wallet.Balance < postPackage.Price)
                {
                    return new BaseResponse
                    {
                        Status = "400",
                        Message = "Số dư ví không đủ để đăng bài.",
                        Data = null
                    };
                }

                // Trừ tiền trong ví
                var walletResult = await _walletRepository.TryUpdateBalanceAsync(wallet.WalletId, -postPackage.Price);
                if (!walletResult.Success)
                {
                    return new BaseResponse
                    {
                        Status = "400",
                        Message = "Cập nhật số dư ví thất bại.",
                        Data = null
                    };
                }

                // Tạo giao dịch WalletTransaction
                var transaction = new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    WalletId = wallet.WalletId,
                    TransactionType = "POSTING",
                    PaymentMethod = "WALLET",
                    Amount = postPackage.Price,
                    BalanceBefore = wallet.Balance,
                    BalanceAfter = wallet.Balance - postPackage.Price,
                    Description = $"Trừ phí đăng bài ({postPackage.PackageName})",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.CreateAsync(transaction);

                // Tạo bài đăng
                var newPost = new Post
                {
                    PostId = Guid.NewGuid(),
                    PackageId = postPackage.PackageId,
                    PackagePrice = postPackage.Price, // lưu giá gói tại thời điểm mua
                    UserId = userId,
                    Type = PostTypeEnum.BATTERY.ToString(),
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    CreatedAt = DateTime.UtcNow,
                    ExpireAt = DateTime.UtcNow.AddDays(postPackage.DurationInDays), // tính ngày hết hạn
                    Status = PostStatusEnum.PENDING.ToString(),
                    PostImages = new List<PostImage>(),
                    Battery = new Battery
                    {
                        BatteryId = Guid.NewGuid(),
                        BrandId = request.batteryCreateDto.BranId,
                        Capacity = request.batteryCreateDto.Capacity,
                        Condition = request.batteryCreateDto.Condition
                    }
                };

                // Upload ảnh
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
                                UploadedAt = DateTime.UtcNow,
                            };
                            newPost.PostImages.Add(postImage);
                        }
                    }
                }

                await _postRepository.CreateAsync(newPost);

                // Map sang DTO
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
                var userId = _userUtility.GetUserIdFromToken();
                if (userId == Guid.Empty)
                    throw new UnauthorizedAccessException("User ID not found in token.");

                // Lấy ví người dùng
                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                    throw new Exception("Wallet not found for the user.");

                // Lấy thông tin gói đăng bài
                var postPackage = await _postPackageRepository.GetByIdAsync(request.postPackgeID);
                if (postPackage == null || !postPackage.IsActive.GetValueOrDefault())
                    throw new Exception("Package not found or inactive.");

                // Kiểm tra số dư
                if (wallet.Balance < postPackage.Price)
                {
                    return new BaseResponse
                    {
                        Status = "400",
                        Message = "Số dư ví không đủ để đăng bài.",
                        Data = null
                    };
                }

                // Trừ tiền trong ví
                var walletResult = await _walletRepository.TryUpdateBalanceAsync(wallet.WalletId, -postPackage.Price);
                if (!walletResult.Success)
                {
                    return new BaseResponse
                    {
                        Status = "400",
                        Message = "Cập nhật số dư ví thất bại.",
                        Data = null
                    };
                }

                // Tạo giao dịch WalletTransaction
                var transaction = new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    WalletId = wallet.WalletId,
                    TransactionType = "POSTING",
                    Amount = postPackage.Price,
                    BalanceBefore = wallet.Balance,
                    PaymentMethod = "WALLET",
                    BalanceAfter = wallet.Balance - postPackage.Price,
                    Description = $"Trừ phí đăng bài ({postPackage.PackageName})",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.CreateAsync(transaction);

                // Tạo bài đăng
                var newPost = new Post
                {
                    PostId = Guid.NewGuid(),
                    PackageId = postPackage.PackageId,
                    PackagePrice = postPackage.Price,
                    UserId = userId,
                    Type = PostTypeEnum.VEHICLE.ToString(),
                    Title = request.Title,
                    Description = request.Description,
                    Price = request.Price,
                    CreatedAt = DateTime.UtcNow,
                    ExpireAt = DateTime.UtcNow.AddDays(postPackage.DurationInDays),
                    Status = PostStatusEnum.PENDING.ToString(),

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

                // Upload ảnh
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
                                UploadedAt = DateTime.UtcNow,
                            };
                            newPost.PostImages.Add(postImage);
                        }
                    }
                }

                await _postRepository.CreateAsync(newPost);

                // DTO trả về
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
                },
                PostDetail = new PostPackgeDTOResponse
                {
                    Id = p.Package.PackageId,
                    PackageName = p.Package.PackageName,
                    Price = p.Package.Price,
                    DurationInDays = p.Package.DurationInDays,
                    CreatedAt = p.Package.CreatedAt,
                    isActive = p.Package.IsActive
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

        public async Task<BaseResponse> GetPostsByDateAndStatusAsync(int day, int month, int year, PostStatusEnum status)
        {
            try
            {
                var posts = await _postRepository.GetPostsByDateAndStatusAsync(day, month, year, status);

                var response = posts.Select(MapToPostDTO).ToList();

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = $"Found {response.Count} posts on {day}/{month}/{year} with status {status}.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = ex.Message
                };
            }
        }

        public async Task<BaseResponse> GetPostsByMonthAndStatusAsync(int month, int year, PostStatusEnum status)
        {
            try
            {
                var posts = await _postRepository.GetPostsByMonthAndStatusAsync(month, year, status);

                var response = posts.Select(MapToPostDTO).ToList();

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = $"Found {response.Count} posts in {month}/{year} with status {status}.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = ex.Message
                };
            }
        }

        public async Task<BaseResponse> GetPostsByYearAndStatusAsync(int year, PostStatusEnum status)
        {
            try
            {
                var posts = await _postRepository.GetPostsByYearAndStatusAsync(year, status);

                var response = posts.Select(MapToPostDTO).ToList();

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = $"Found {response.Count} posts in year {year} with status {status}.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = ex.Message
                };
            }
        }

        // Từ chối bài đăng và hoàn tiền
        public async Task<BaseResponse> RejectStatusAsync(Guid postId)
        {
            try
            {
                // Lấy bài đăng
                var post = await _postRepository.GetByIdAsync(postId);
                if (post == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Post not found."
                    };
                }

                // Cập nhật trạng thái bài đăng
                post.Status = PostStatusEnum.REJECTED.ToString();
                await _postRepository.UpdateAsync(post);

                // Kiểm tra UserId hợp lệ
                if (post.UserId == null || post.UserId == Guid.Empty)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status500InternalServerError.ToString(),
                        Message = "UserId of post is invalid."
                    };
                }

                // Lấy ví người dùng
                var userWallet = await _walletRepository.GetWalletByUserIdAsync(post.UserId);
                if (userWallet == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status500InternalServerError.ToString(),
                        Message = "User wallet not found."
                    };
                }

                // Lấy giá gói đăng bài
                var postPackage = await _postPackageRepository.GetByIdAsync(post.PackageId);
                if (postPackage == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status500InternalServerError.ToString(),
                        Message = "Post package not found."
                    };
                }

                // Hoàn tiền về ví người đăng bài
                var walletResult = await _walletRepository.TryUpdateBalanceAsync(userWallet.WalletId, postPackage.Price);
                if (!walletResult.Success)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status500InternalServerError.ToString(),
                        Message = "Failed to refund user wallet."
                    };
                }

                // Tạo WalletTransaction ghi lại giao dịch hoàn tiền
                var transaction = new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    WalletId = userWallet.WalletId,
                    TransactionType = "POST_REJECT_REFUND",
                    Amount = postPackage.Price,
                    BalanceBefore = userWallet.Balance,
                    BalanceAfter = userWallet.Balance + postPackage.Price,
                    Description = $"Hoàn tiền gói đăng bài ({postPackage.PackageName}) do bài bị từ chối",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.CreateAsync(transaction);

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = $"Post rejected and {postPackage.Price} refunded to user.",
                    Data = new
                    {
                        PostId = post.PostId,
                        UserNewBalance = walletResult.NewBalance
                    }
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

        private PostResponseDto MapToPostDTO(Post p)
        {
            return new PostResponseDto
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

                Vehicle = p.Vehicle != null ? new VehicleDto
                {
                    VehicleId = p.Vehicle.VehicleId,
                    BrandName = p.Vehicle.Brand?.Name ?? "Unknown",
                    Model = p.Vehicle.Model ?? "",
                    Year = p.Vehicle.Year,
                    Mileage = p.Vehicle.Mileage
                } : null,

                Battery = p.Battery != null ? new BatteryDto
                {
                    BatteryId = p.Battery.BatteryId,
                    BrandName = p.Battery.Brand?.Name ?? "Unknown",
                    Capacity = p.Battery.Capacity,
                    Condition = p.Battery.Condition ?? ""
                } : null,

                User = p.User != null ? new UserinformationResponse
                {
                    UserId = p.User.UserId,
                    FullName = p.User.FullName,
                    Email = p.User.Email,
                    Phone = p.User.Phone,
                    Status = p.User.Status,
                    Role = p.User.Role
                } : null
            };
        }

    }
}
