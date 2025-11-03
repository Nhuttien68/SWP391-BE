using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
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

                // ✅ Kiểm tra đã review chưa
                bool exists = await _reviewRepository.ExistsAsync(dto.TransactionId, "Post");
                if (exists)
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status409Conflict.ToString(),
                        Message = "You have already reviewed this post for this transaction"
                    };

                // ✅ Tạo review mới
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
                    Data = new
                    {
                        review.ReviewId,
                        review.Rating,
                        review.Comment,
                        review.CreatedAt
                    }
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

        public async Task<BaseResponse> DeleteReviewAsync(Guid reviewId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> GetByPostIdAsync(Guid postId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> GetByTransactionIdAsync(Guid transactionId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse> GetByUserIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }
    }
}
