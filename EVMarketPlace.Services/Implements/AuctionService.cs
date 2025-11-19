using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVMarketPlace.Services.Implements
{
    public class AuctionService : IAuctionService
    {
        private readonly AuctionRepository _auctionRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly IWalletService _walletService;
        private readonly ILogger<AuctionService> _logger;
        private readonly UserUtility _userUtility;

        public AuctionService(
            AuctionRepository auctionRepository,
            TransactionRepository transactionRepository,
            IWalletService walletService,
            ILogger<AuctionService> logger, UserUtility userUtility)
        {
            _auctionRepository = auctionRepository;
            _transactionRepository = transactionRepository;
            _walletService = walletService;
            _logger = logger;
            _userUtility = userUtility;
        }

        public async Task<BaseResponse> CreateAuctionAsync(CreateAuctionRequest req)
        {
            try
            {
                var userId = _userUtility.GetUserIdFromToken();
                var post = await _auctionRepository.GetPostByIdAsync(req.PostId);
                if (post == null)
                    return new BaseResponse { Status = "404", Message = "Post not found" };

                if (post.UserId != userId)
                    return new BaseResponse { Status = "403", Message = "You are not allowed to create an auction for this post" };

                // Kiểm tra post đã có auction chưa
                var hasAuction = await _auctionRepository.PostHasAuctionAsync(req.PostId);
                if (hasAuction)
                    return new BaseResponse { Status = "400", Message = "This post already has an auction" };

                // Kiểm tra post phải được duyệt mới tạo đấu giá được
                if (post.Status != PostStatusEnum.APPROVED.ToString())
                    return new BaseResponse { Status = "400", Message = "Post must be approved before creating an auction" };

                if (req.EndTime <= DateTime.UtcNow)
                    return new BaseResponse { Status = "400", Message = "End time must be in the future" };

                var auction = new Auction
                {
                    AuctionId = Guid.NewGuid(),
                    PostId = req.PostId,
                    StartPrice = req.StartPrice,
                    CurrentPrice = req.StartPrice,
                    EndTime = req.EndTime,
                    Status = "Active"
                };

                await _auctionRepository.CreateAsync(auction);

                return new BaseResponse
                {
                    Status = "201",
                    Message = "Auction created successfully",
                    Data = new
                    {
                        AuctionId = auction.AuctionId,
                        auction.PostId,
                        auction.StartPrice,
                        auction.CurrentPrice,
                        auction.EndTime,
                        auction.Status
                    }
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized: {Message}", ex.Message);
                return new BaseResponse { Status = "401", Message = "Unauthorized - Invalid token" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating auction for PostId: {PostId}", req.PostId);
                return new BaseResponse { Status = "500", Message = "An unexpected error occurred while creating auction" };
            }
        }

        public async Task<BaseResponse> PlaceBidAsync( PlaceBidRequest req)
        {
            var auction = await _auctionRepository.GetAuctionWithBidsAsync(req.AuctionId);
            if (auction == null)
                return new BaseResponse { Status = "404", Message = "Auction not found" };
            var userId = _userUtility.GetUserIdFromToken();

            if (auction.Status != "Active")
                return new BaseResponse { Status = "400", Message = "Auction not active" };

            if (DateTime.UtcNow >= auction.EndTime)
                return new BaseResponse { Status = "400", Message = "Auction has ended" };

            if (req.BidAmount <= auction.CurrentPrice)
                return new BaseResponse { Status = "400", Message = "Bid must be higher than current price" };

            var bid = new AuctionBid
            {
                BidId = Guid.NewGuid(),
                AuctionId = auction.AuctionId,
                UserId = userId,
                BidAmount = req.BidAmount,
                BidTime = DateTime.UtcNow
            };

            auction.CurrentPrice = req.BidAmount;

            await _auctionRepository.AddBidAsync(bid);
            await _auctionRepository.UpdateAsync(auction);

            return new BaseResponse
            {
                Status = "200",
                Message = "Bid placed successfully",
                Data = bid
            };
        }

        public async Task<List<AuctionCloseResultDTO>> CloseExpiredAuctionsAsync()
        {
            var expired = await _auctionRepository.GetExpiredAuctionsAsync();
            var results = new List<AuctionCloseResultDTO>();

            foreach (var auction in expired)
            {
                auction.Status = "Ended";
                var highestBid = auction.AuctionBids
     .OrderByDescending(b => b.BidAmount)
     .FirstOrDefault();

                auction.Status = "Ended";

                if (highestBid == null || highestBid.UserId == null || highestBid.BidAmount == null)
                {
                    _logger.LogWarning("⚠️ Auction {AuctionId} has no valid bids.", auction.AuctionId);
                    continue;
                }

                if (auction.Post == null)
                {
                    _logger.LogWarning("⚠️ Auction {AuctionId} missing post data.", auction.AuctionId);
                    continue;
                }

                // ✅ Trừ tiền người thắng
                var deduct = await _walletService.DeductAsync(highestBid.UserId.Value, highestBid.BidAmount.Value);
                if (deduct.Status != "200")
                {
                    _logger.LogWarning("⚠️ Không thể trừ tiền người thắng {UserId}: {Message}", highestBid.UserId, deduct.Message);
                    continue;
                }

                // ✅ Tạo transaction
                var trans = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    BuyerId = highestBid.UserId.Value,
                    SellerId = auction.Post.UserId,
                    PostId = auction.PostId,
                    Amount = highestBid.BidAmount.Value,
                    PaymentMethod = "Wallet",
                    Status = "Paid",
                    CreatedAt = DateTime.UtcNow
                };
                await _transactionRepository.CreateAsync(trans);

            }

            return results;
        }

        public async Task<List<Auction>> GetActiveAuctionsAsync()
        {
            return await _auctionRepository.GetActiveAuctionsAsync();
        }

        public async Task<BaseResponse?> GetAuctionByIdAsync(Guid auctionId)
        {
            var auction = await _auctionRepository.GetByIdAsync(auctionId);
            if (auction == null)
                return new BaseResponse { Status = "404", Message = "Auction not found" };

            return new BaseResponse { Status = "200", Message = "Success", Data = auction };
        }


        public async Task<BaseResponse> UpdateTransactionReceiverInfoAsync(Guid transactionId, UpdateTransactionRequest req)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
                return new BaseResponse { Status = "404", Message = "Transaction not found" };

            transaction.ReceiverName = req.ReceiverName;
            transaction.ReceiverPhone = req.ReceiverPhone;
            transaction.ReceiverAddress = req.ReceiverAddress;
            transaction.Note = req.Note;

            await _transactionRepository.UpdateAsync(transaction);

            return new BaseResponse
            {
                Status = "200",
                Message = "Receiver info updated successfully"
            };
        }
    }
}
