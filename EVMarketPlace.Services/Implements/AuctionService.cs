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
        private readonly WalletRepository _walletRepository;
        private readonly WalletTransactionRepository _walletTransactionRepository;

        public AuctionService(
            AuctionRepository auctionRepository,
            TransactionRepository transactionRepository,
            IWalletService walletService,
            WalletRepository walletRepository,
            WalletTransactionRepository walletTransactionRepository,
            ILogger<AuctionService> logger, UserUtility userUtility)
        {
            _auctionRepository = auctionRepository;
            _transactionRepository = transactionRepository;
            _walletService = walletService;
            _logger = logger;
            _userUtility = userUtility;
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;

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

                if (req.EndTime <= DateTime.Now)
                    return new BaseResponse { Status = "400", Message = "End time must be in the future" };

                var auction = new Auction
                {
                    AuctionId = Guid.NewGuid(),
                    PostId = req.PostId,
                    StartPrice = req.StartPrice,
                    BidStep = req.BidStep,
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
                        auction.BidStep,
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

        public async Task<BaseResponse> PlaceBidAsync(PlaceBidRequest req)
        {
            var auction = await _auctionRepository.GetAuctionWithBidsAsync(req.AuctionId);
            if (auction == null)
                return new BaseResponse { Status = "404", Message = "Auction not found" };

            var userId = _userUtility.GetUserIdFromToken();

            if (auction.Status != "Active")
                return new BaseResponse { Status = "400", Message = "Auction not active" };

            if (DateTime.Now >= auction.EndTime)
                return new BaseResponse { Status = "400", Message = "Auction has ended" };

            // Không được đấu giá bài của mình
            if (auction.Post != null && auction.Post.UserId == userId)
                return new BaseResponse { Status = "400", Message = "You cannot bid on your own auction" };

            if (req.BidAmount <= auction.CurrentPrice)
                return new BaseResponse { Status = "400", Message = "Bid must be higher than current price" };

            // ⭐ LẤY VÍ NGƯỜI ĐẤU GIÁ
            var userWallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (userWallet == null)
                return new BaseResponse { Status = "500", Message = "Wallet not found" };

            // ⭐ CHECK SỐ DƯ
            if (userWallet.Balance < req.BidAmount)
            {
                return new BaseResponse
                {
                    Status = "400",
                    Message = "Your wallet balance is not enough to place this bid"
                };
            }

            // Tạo bid
            var bid = new AuctionBid
            {
                BidId = Guid.NewGuid(),
                AuctionId = auction.AuctionId,
                UserId = userId,
                BidAmount = req.BidAmount,
                BidTime = DateTime.Now
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
                // ⛔ Nếu đã xử lý rồi thì bỏ qua
                if (auction.Status != "Active") continue;

                // 🟡 Đánh dấu đang xử lý
                auction.Status = "Processing";
                await _auctionRepository.UpdateAsync(auction);

                // 🏅 Lấy người đặt giá cao nhất
                var highestBid = auction.AuctionBids
                    .OrderByDescending(b => b.BidAmount)
                    .FirstOrDefault();

                if (highestBid == null || highestBid.UserId == null)
                {
                    auction.Status = "Ended";
                    await _auctionRepository.UpdateAsync(auction);
                    continue;
                }

                // ❗ Lấy post
                if (auction.Post == null)
                {
                    auction.Status = "Failed";
                    await _auctionRepository.UpdateAsync(auction);
                    continue;
                }

                // 🔹 Lấy ví người thắng
                var winnerWallet = await _walletRepository.GetWalletByUserIdAsync(highestBid.UserId.Value);
                if (winnerWallet == null)
                {
                    auction.Status = "Failed";
                    await _auctionRepository.UpdateAsync(auction);
                    continue;
                }

                // ❌ Trừ tiền người thắng
                var deduct = await _walletService.DeductAsync(highestBid.UserId.Value, highestBid.BidAmount.Value);
                if (deduct.Status != "200")
                {
                    auction.Status = "Failed";
                    await _auctionRepository.UpdateAsync(auction);
                    continue;
                }

                // 🟢 Tạo WalletTransaction cho người thắng
                var buyerWalletTran = new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    WalletId = winnerWallet.WalletId,
                    Amount = -highestBid.BidAmount.Value,
                    TransactionType = "AuctionPayment",
                    Description = $"Payment for auction {auction.AuctionId}",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.CreateAsync(buyerWalletTran);

                // 🔹 Lấy ví người bán
                var sellerWallet = await _walletRepository.GetWalletByUserIdAsync(auction.Post.UserId);
                if (sellerWallet == null)
                {
                    // Hoàn lại tiền cho người thắng nếu seller không có ví
                    string refundTransId = $"REFUND_{auction.AuctionId}_{DateTime.UtcNow.Ticks}";
                    await _walletService.TopUpWalletAsync(highestBid.BidAmount.Value, refundTransId, "AuctionRefund", highestBid.UserId.Value);

                    auction.Status = "Failed";
                    await _auctionRepository.UpdateAsync(auction);
                    continue;
                }

                // 🟢 Cộng tiền cho seller
                string auctionTransId = $"AUCTION_{auction.AuctionId}_{DateTime.UtcNow.Ticks}";
                var addToSeller = await _walletService.TopUpWalletAsync(
                    highestBid.BidAmount.Value,
                    auctionTransId,
                    "AuctionPayout",
                    auction.Post.UserId
                );

                if (addToSeller.Status != "200")
                {
                    // ❗ Hoàn lại tiền cho người thắng nếu seller nhận tiền thất bại
                    string refundTransId = $"REFUND_{auction.AuctionId}_{DateTime.UtcNow.Ticks}";
                    await _walletService.TopUpWalletAsync(highestBid.BidAmount.Value, refundTransId, "AuctionRefund", highestBid.UserId.Value);

                    auction.Status = "Failed";
                    await _auctionRepository.UpdateAsync(auction);
                    continue;
                }

                // 🟢 Tạo WalletTransaction cho seller
                var sellerWalletTran = new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    WalletId = sellerWallet.WalletId,
                    Amount = highestBid.BidAmount.Value,
                    TransactionType = "AuctionPayout",
                    Description = $"Seller payout for auction {auction.AuctionId}",
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.CreateAsync(sellerWalletTran);

                // 🏆 Gán người thắng
                auction.WinnerId = highestBid.UserId.Value;
                auction.Status = "Ended";
                await _auctionRepository.UpdateAsync(auction);

                // 🧾 Tạo transaction chính
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

                // Thêm kết quả vào danh sách trả về
                results.Add(new AuctionCloseResultDTO
                {
                    AuctionId = auction.AuctionId,
                    WinnerId = auction.WinnerId,
                    FinalAmount = highestBid.BidAmount.Value,
                    Status = auction.Status
                });
            }

            return results;
        }



        public async Task<List<Auction>> GetActiveAuctionsAsync()
        {
            return await _auctionRepository.GetActiveAuctionsAsync();
        }

        public async Task<BaseResponse?> GetAuctionByIdAsync(Guid auctionId)
        {
            var auction = await _auctionRepository.GetAuctionWithBidsAsync(auctionId);
            if (auction == null)
                return new BaseResponse { Status = "404", Message = "Auction not found" };

            var detail = new AuctionDetailDTO
            {
                AuctionId = auction.AuctionId,
                PostId = auction.PostId,
                StartPrice = auction.StartPrice,
                CurrentPrice = auction.CurrentPrice,
                EndTime = auction.EndTime,
                Status = auction.Status,
                Post = auction.Post == null ? null : new AuctionPostSummaryDTO
                {
                    PostId = auction.Post.PostId,
                    Title = auction.Post.Title,
                    Description = auction.Post.Description,
                    CreatedAt = auction.Post.CreatedAt,
                    ImageUrls = auction.Post.PostImages?.Select(pi => pi.ImageUrl).ToList()
                },
                AuctionBids = auction.AuctionBids
                    .OrderByDescending(b => b.BidTime)
                    .Select(b => new AuctionBidHistoryItemDTO
                    {
                        BidId = b.BidId,
                        UserId = b.UserId,
                        UserName = b.User?.FullName,
                        BidAmount = b.BidAmount,
                        BidTime = b.BidTime
                    }).ToList()
            };

            return new BaseResponse { Status = "200", Message = "Success", Data = detail };
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
