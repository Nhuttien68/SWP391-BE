using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.ResponseDTO.Posts;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{
    public class ReviewService : IReviewService
    {
       private readonly ReviewRepository _reviewRepository;
       private readonly TransactionRepository _transactionRepo;
        private readonly UserUtility _userUtility;
        public ReviewService(ReviewRepository reviewRepository, UserUtility userUtility,TransactionRepository transactionRepository)
        {
            _reviewRepository = reviewRepository;
            _transactionRepo = transactionRepository;
            _userUtility = userUtility;
        }
        // Tạo đánh giá cho bài đăng

        public async Task<BaseResponse> CreateReviewForPostAsync(ReviewCreateDTO dto)
        {
            try
            {
                var transaction = await _transactionRepo.GetTransactionWithPostAsync(dto.TransactionId);
                var reviewerId = _userUtility.GetUserIdFromToken();

                if (transaction == null)
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Transaction not found"
                    };

                if (transaction.BuyerId != reviewerId)
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status403Forbidden.ToString(),
                        Message = "You are not the buyer of this transaction"
                    };

                if (!transaction.Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase))
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status400BadRequest.ToString(),
                        Message = "Transaction is not completed"
                    };
                bool exists = await _reviewRepository.ExistsAsync(dto.TransactionId, "Post");
                if (exists)
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status409Conflict.ToString(),
                        Message = "You have already reviewed this post for this transaction"
                    };
 
                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    TransactionId = dto.TransactionId,
                    ReviewerId = reviewerId,
                    PostId = transaction.PostId,
                    ReviewTargetType = ReviewTypeEnum.PostReview.ToString(),
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                await _reviewRepository.CreateAsync(review);

                return new BaseResponse
                {
                    Status = StatusCodes.Status201Created.ToString(),
                    Message = "Post review created successfully",
                  
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"An error occurred while creating the post review: {ex.Message}"
                };
            }
        }

        // Tạo đánh giá cho người bán
        public async Task<BaseResponse> CreateReviewForSellerAsync(ReviewCreateDTO dto)
        {
            try
            {
                
                var transaction = await _transactionRepo.GetTransactionWithSellerAsync(dto.TransactionId);
                var reviewerId = _userUtility.GetUserIdFromToken();

                if (transaction == null)
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Transaction not found"
                    };

                if (transaction.BuyerId != reviewerId)
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status403Forbidden.ToString(),
                        Message = "You are not the buyer of this transaction"
                    };

                if (!transaction.Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase))
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status400BadRequest.ToString(),
                        Message = "Transaction is not completed"
                    };

                
                bool exists = await _reviewRepository.ExistsAsync(dto.TransactionId, "Seller");
                if (exists)
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status409Conflict.ToString(),
                        Message = "You have already reviewed this seller for this transaction"
                    };

               
                var review = new Review
                {
                    ReviewId = Guid.NewGuid(),
                    TransactionId = dto.TransactionId,
                    ReviewerId = reviewerId,
                    ReviewedUserId = transaction.SellerId,
                    ReviewTargetType = ReviewTypeEnum.SellerReview.ToString(),
                    Rating = dto.Rating,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                await _reviewRepository.CreateAsync(review);

                return new BaseResponse
                {
                    Status = StatusCodes.Status201Created.ToString(),
                    Message = "Seller review created successfully",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"An error occurred while creating the seller review: {ex.Message}"
                };
            }
        }
        // Xóa đánh giá
        public async Task<BaseResponse> DeleteReviewAsync(Guid reviewId)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(reviewId);
                if (review == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Review not found."
                    };
                }

                await _reviewRepository.RemoveAsync(review);

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Review deleted successfully."
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Error deleting review: {ex.Message}"
                };
            }
        }
        // Lấy đánh giá theo postId
        public async Task<BaseResponse> GetByPostIdAsync(Guid postId)
        {
            try
            {
                var reviews = await _reviewRepository.GetReviewsByPostIdAsync(postId);

                if (!reviews.Any())
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "No reviews found for this post."
                    };
                }

                var reviewDtos = reviews.Select(r => new ReviewPostResponseDTO
                {
                    ReviewId = r.ReviewId,
                    ReviewTargetType = r.ReviewTargetType ?? "",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt ?? DateTime.UtcNow,
                    ReviewerName = r.Reviewer?.FullName ?? "N/A",
                    detailDto = r.Post != null
                        ? new PostResponseDto
                        {
                            PostId = r.Post.PostId,
                            Title = r.Post.Title,
                            Description = r.Post.Description,
                            Price = r.Post.Price,
                            Type = r.Post.Type,
                            CreatedAt = r.Post.CreatedAt,
                            Status = r.Post.Status,
                            ImgId = r.Post.PostImages?.Select(i => i.ImageId).ToList(),
                            ImageUrls = r.Post.PostImages?.Select(i => i.ImageUrl).ToList(),

                            // Nếu là bài đăng về xe
                            Vehicle = r.Post.Vehicle != null ? new VehicleDto
                            {
                                VehicleId = r.Post.Vehicle.VehicleId,
                                BrandName = r.Post.Vehicle.Brand?.Name ?? "Unknown",
                                Model = r.Post.Vehicle.Model ?? "",
                                Year = r.Post.Vehicle.Year,
                                Mileage = r.Post.Vehicle.Mileage
                            } : null,

                            // Nếu là bài đăng về pin
                            Battery = r.Post.Battery != null ? new BatteryDto
                            {
                                BatteryId = r.Post.Battery.BatteryId,
                                BrandName = r.Post.Battery.Brand?.Name ?? "Unknown",
                                Capacity = r.Post.Battery.Capacity,
                                Condition = r.Post.Battery.Condition ?? ""
                            } : null,

                            User = new UserinformationResponse
                            {
                                UserId = r.Post.User.UserId,
                                FullName = r.Post.User.FullName,
                                Email = r.Post.User.Email,
                                Phone = r.Post.User.Phone,
                                Status = r.Post.User.Status,
                                Role = r.Post.User.Role
                            }
                        }
                : null
                }).ToList();

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Reviews retrieved successfully.",
                    Data = reviewDtos
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Error retrieving reviews: {ex.Message}"
                };
            }
        }
        // Lấy đánh giá theo người dùng (người bán)
        public async Task<BaseResponse> GetByUserIdAsync(Guid userId)
        {
            try
            {
                var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);

                if (!reviews.Any())
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "No reviews found for this user."
                    };
                }

                var reviewDtos = reviews.Select(r => new ReviewSellerResponseDTO
                {
                    ReviewId = r.ReviewId,
                    ReviewTargetType = r.ReviewTargetType ?? "",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt ?? DateTime.UtcNow,
                    ReviewerName = r.Reviewer?.FullName ?? "N/A",
                    SellerInfor = r.ReviewedUser != null
                        ? new UserinformationResponse
                        {
                            UserId = r.ReviewedUser.UserId,
                            FullName = r.ReviewedUser.FullName,
                            Email = r.ReviewedUser.Email,
                            Phone = r.ReviewedUser.Phone,
                            Status = r.ReviewedUser.Status
                            
                        }
                        : null
                }).ToList();

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Seller reviews retrieved successfully.",
                    Data = reviewDtos
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Error retrieving seller reviews: {ex.Message}"
                };
            }
        }

        public async Task<BaseResponse> UpdateReviewAsync(UpdateReviewDTO dto)
        {
            try
            {
                var review = await _reviewRepository.GetByIdAsync(dto.ReviewId);
                if (review == null)
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status404NotFound.ToString(),
                        Message = "Review not found."
                    };
                }

                review.Rating = dto.Rating;
                review.Comment = dto.Comment;     

                await _reviewRepository.UpdateAsync(review);

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Review updated successfully.",
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Error updating review: {ex.Message}"
                };
            }
        }
    }
}
