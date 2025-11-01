﻿using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Enums;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Interfaces;
using System.Security.Claims;

namespace EVMarketPlace.Services.Implements
{
    public class TransactionService : ITransactionService
    {
        private readonly TransactionRepository _transactionRepository;
        private readonly PostRepository _postRepository;
        private readonly IWalletService _walletService;

        public TransactionService(
            TransactionRepository transactionRepository,
            PostRepository postRepository,
            IWalletService walletService)
        {
            _transactionRepository = transactionRepository;
            _postRepository = postRepository;
            _walletService = walletService;
        }

        // Tạo giao dịch mới
        public async Task<BaseResponse> CreateTransactionAsync(ClaimsPrincipal user, CreateTransactionRequest request)
        {
            try
            {
                var userId = GetUserId(user);
                if (userId == Guid.Empty)
                {
                    return Response(401, "Người dùng chưa xác thực.");
                }

                // Validate payment method
                if (!Enum.TryParse<PaymentMethodEnum>(request.PaymentMethod, true, out _))
                {
                    return Response(400, "Phương thức thanh toán không hợp lệ.");
                }

                // Lấy thông tin sản phẩm
                var post = await _postRepository.GetByIdAsync(request.PostId);
                if (post == null)
                {
                    return Response(404, "Sản phẩm không tồn tại.");
                }

                // Không thể mua sản phẩm của chính mình
                if (post.UserId == userId)
                {
                    return Response(400, "Bạn không thể mua sản phẩm của chính mình.");
                }

                // Chỉ mua được sản phẩm đã APPROVED
                if (post.Status != PostStatusEnum.APPROVED.ToString())
                {
                    var statusMessage = post.Status switch
                    {
                        var s when s == PostStatusEnum.PENNDING.ToString() => "Sản phẩm đang chờ duyệt.",
                        var s when s == PostStatusEnum.REJECTED.ToString() => "Sản phẩm đã bị từ chối.",
                        var s when s == PostStatusEnum.SOLD.ToString() => "Sản phẩm đã được bán.",
                        var s when s == PostStatusEnum.DELETED.ToString() => "Sản phẩm đã bị xóa.",
                        _ => "Sản phẩm không có sẵn."
                    };
                    return Response(400, statusMessage);
                }

                // Kiểm tra số dư ví người mua
                var buyerBalance = await _walletService.GetBalanceAsync();
                if (buyerBalance < post.Price)
                {
                    return Response(400, $"Số dư không đủ. Cần {post.Price:N0} VNĐ, hiện có {buyerBalance:N0} VNĐ.");
                }

                // Trừ tiền người mua
                var deductResult = await _walletService.DeductAsync(userId, post.Price ?? 0);
                if (int.Parse(deductResult.Status) != 200)
                {
                    return Response(400, "Không thể trừ tiền từ ví. Vui lòng thử lại.");
                }

                // Tạo transaction
                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    BuyerId = userId,
                    SellerId = post.UserId,
                    PostId = request.PostId,
                    Amount = post.Price,
                    PaymentMethod = request.PaymentMethod,
                    Status = TransactionStatusEnum.PENDING.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    ReceiverName = request.ReceiverName,
                    ReceiverPhone = request.ReceiverPhone,
                    ReceiverAddress = request.ReceiverAddress,
                    Note = request.Note
                };

                await _transactionRepository.CreateAsync(transaction);

                var response = await MapToDTO(transaction);
                return Response(201, "Tạo giao dịch thành công. Tiền đã được trừ từ ví.", response);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        // Lấy chi tiết giao dịch
        public async Task<BaseResponse> GetTransactionByIdAsync(ClaimsPrincipal user, Guid transactionId)
        {
            try
            {
                var userId = GetUserId(user);
                if (userId == Guid.Empty)
                {
                    return Response(401, "Người dùng chưa xác thực.");
                }

                var transaction = await _transactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return Response(404, "Giao dịch không tồn tại.");
                }

                // Kiểm tra quyền xem
                var role = GetRole(user);
                if (role != "ADMIN" && transaction.BuyerId != userId && transaction.SellerId != userId)
                {
                    return Response(403, "Bạn không có quyền xem giao dịch này.");
                }

                var response = await MapToDTO(transaction);
                return Response(200, "Lấy thông tin giao dịch thành công.", response);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        // Lấy danh sách đơn mua
        public async Task<BaseResponse> GetMyPurchasesAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = GetUserId(user);
                if (userId == Guid.Empty)
                {
                    return Response(401, "Người dùng chưa xác thực.");
                }

                var transactions = await _transactionRepository.GetByBuyerIdAsync(userId);
                var response = transactions.Select(MapToListItemDTO).ToList();

                return Response(200, $"Tổng: {response.Count} đơn mua.", response);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        // Lấy danh sách đơn bán
        public async Task<BaseResponse> GetMySalesAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = GetUserId(user);
                if (userId == Guid.Empty)
                {
                    return Response(401, "Người dùng chưa xác thực.");
                }

                var transactions = await _transactionRepository.GetBySellerIdAsync(userId);
                var response = transactions.Select(MapToListItemDTO).ToList();

                return Response(200, $"Tổng: {response.Count} đơn bán.", response);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        // Cập nhật trạng thái giao dịch
        public async Task<BaseResponse> UpdateTransactionStatusAsync(ClaimsPrincipal user, UpdateTransactionStatusRequest request)
        {
            try
            {
                var userId = GetUserId(user);
                if (userId == Guid.Empty)
                {
                    return Response(401, "Người dùng chưa xác thực.");
                }

                // Validate status
                if (!Enum.TryParse<TransactionStatusEnum>(request.Status, true, out _))
                {
                    return Response(400, "Trạng thái không hợp lệ.");
                }

                var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);
                if (transaction == null)
                {
                    return Response(404, "Giao dịch không tồn tại.");
                }

                // Chỉ seller hoặc admin có thể cập nhật
                var role = GetRole(user);
                if (role != "ADMIN" && transaction.SellerId != userId)
                {
                    return Response(403, "Chỉ người bán hoặc Admin mới có thể cập nhật.");
                }

                // Kiểm tra trạng thái hiện tại
                if (transaction.Status == TransactionStatusEnum.COMPLETED.ToString())
                {
                    return Response(400, "Giao dịch đã hoàn thành, không thể cập nhật.");
                }

                if (transaction.Status == TransactionStatusEnum.CANCELLED.ToString())
                {
                    return Response(400, "Giao dịch đã hủy, không thể cập nhật.");
                }

                // Cập nhật trạng thái
                transaction.Status = request.Status;

                // Nếu hoàn thành → cộng tiền cho seller
                if (request.Status == TransactionStatusEnum.COMPLETED.ToString())
                {
                    //Lấy post bằng GetByIdAsync có sẵn
                    var post = await _postRepository.GetByIdAsync(transaction.PostId.Value);

                    if (post != null)
                    {
                        //Update status
                        post.Status = PostStatusEnum.SOLD.ToString();

                        //Save changes
                        await _postRepository.UpdateAsync(post);
                    }

                    // Cộng tiền vào ví người bán
                    var topUpResult = await _walletService.TopUpWalletAsync(
                        transaction.Amount ?? 0,
                        transaction.TransactionId.ToString(),
                        "TRANSACTION",
                        transaction.SellerId ?? Guid.Empty
                    );

                    if (int.Parse(topUpResult.Status) != 200)
                    {
                        return Response(400, "Không thể cộng tiền cho người bán. Vui lòng thử lại.");
                    }
                }

                await _transactionRepository.UpdateAsync(transaction);

                var response = await MapToDTO(transaction);

                // Thông báo cụ thể theo từng status
                var message = request.Status switch
                {
                    var s when s == TransactionStatusEnum.COMPLETED.ToString() =>
                        "Giao dịch hoàn thành. Tiền đã được chuyển cho người bán.",
                    var s when s == TransactionStatusEnum.PENDING.ToString() =>
                        "Đơn hàng đang chờ xử lý.",
                    _ => "Cập nhật trạng thái thành công."
                };

                return Response(200, message, response);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        // Hủy giao dịch
        public async Task<BaseResponse> CancelTransactionAsync(ClaimsPrincipal user, Guid transactionId)
        {
            try
            {
                var userId = GetUserId(user);
                if (userId == Guid.Empty)
                {
                    return Response(401, "Người dùng chưa xác thực.");
                }

                var transaction = await _transactionRepository.GetByIdAsync(transactionId);
                if (transaction == null)
                {
                    return Response(404, "Giao dịch không tồn tại.");
                }

                // Chỉ buyer hoặc admin có thể hủy
                var role = GetRole(user);
                if (role != "ADMIN" && transaction.BuyerId != userId)
                {
                    return Response(403, "Chỉ người mua hoặc Admin mới có thể hủy.");
                }

                // Không thể hủy giao dịch đã hoàn thành
                if (transaction.Status == TransactionStatusEnum.COMPLETED.ToString())
                {
                    return Response(400, "Không thể hủy giao dịch đã hoàn thành.");
                }

                // Hoàn tiền cho người mua nếu đã trừ tiền
                if (transaction.Status == TransactionStatusEnum.PENDING.ToString())
                {
                    var refundResult = await _walletService.TopUpWalletAsync(
                        transaction.Amount ?? 0,
                        $"REFUND-{transaction.TransactionId}",
                        "REFUND",
                        transaction.BuyerId ?? Guid.Empty
                    );

                    if (int.Parse(refundResult.Status) != 200)
                    {
                        return Response(400, "Không thể hoàn tiền. Vui lòng liên hệ hỗ trợ.");
                    }

                    // 
                    var post = await _postRepository.GetByIdAsync(transaction.PostId.Value);
                    if (post != null && post.Status == PostStatusEnum.SOLD.ToString())
                    {
                        post.Status = PostStatusEnum.APPROVED.ToString();
                        await _postRepository.UpdateAsync(post);
                    }
                }

                transaction.Status = TransactionStatusEnum.CANCELLED.ToString();
                await _transactionRepository.UpdateAsync(transaction);

                return Response(200, "Hủy giao dịch thành công. Tiền đã được hoàn lại và sản phẩm đã mở khóa.");
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        // Admin: Lấy tất cả giao dịch
        public async Task<BaseResponse> GetAllTransactionsAsync(ClaimsPrincipal user)
        {
            try
            {
                var role = GetRole(user);
                if (role != "ADMIN")
                {
                    return Response(401, "Chỉ Admin mới có quyền xem tất cả giao dịch.");
                }

                var transactions = await _transactionRepository.GetAllAsync();
                var response = transactions.Select(MapToListItemDTO).ToList();

                return Response(200, $"Tổng: {response.Count} giao dịch.", response);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        // Helper: Lấy UserId
        private Guid GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("UserId")?.Value;

            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        // Helper: Lấy Role
        private string GetRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        // Helper: Map sang DTO
        private async Task<TransactionResponseDTO> MapToDTO(Transaction t)
        {
            if (t.Buyer == null || t.Seller == null || t.Post == null)
            {
                t = await _transactionRepository.GetByIdAsync(t.TransactionId);
            }

            return new TransactionResponseDTO
            {
                TransactionId = t.TransactionId,
                BuyerId = t.BuyerId ?? Guid.Empty,
                BuyerName = t.Buyer?.FullName ?? "N/A",
                SellerId = t.SellerId ?? Guid.Empty,
                SellerName = t.Seller?.FullName ?? "N/A",
                PostId = t.PostId ?? Guid.Empty,
                PostTitle = t.Post?.Title ?? "N/A",
                Amount = t.Amount ?? 0,
                PaymentMethod = t.PaymentMethod ?? "N/A",
                Status = t.Status ?? "N/A",
                CreatedAt = t.CreatedAt ?? DateTime.UtcNow,
                ReceiverName = t.ReceiverName ?? "N/A",
                ReceiverPhone = t.ReceiverPhone ?? "N/A",
                ReceiverAddress = t.ReceiverAddress ?? "N/A",
                Note = t.Note
            };
        }

        // Helper: Map sang List Item DTO
        private TransactionListItemDTO MapToListItemDTO(Transaction t)
        {
            return new TransactionListItemDTO
            {
                TransactionId = t.TransactionId,
                PostTitle = t.Post?.Title ?? "N/A",
                Amount = t.Amount ?? 0,
                Status = t.Status ?? "N/A",
                PaymentMethod = t.PaymentMethod ?? "N/A",
                CreatedAt = t.CreatedAt ?? DateTime.UtcNow,
                PostImageUrl = t.Post?.PostImages?.FirstOrDefault()?.ImageUrl
            };
        }

        // Helper: Tạo Response
        private BaseResponse Response(int code, string message, object? data = null)
        {
            return new BaseResponse
            {
                Status = code.ToString(),
                Message = message,
                Data = data
            };
        }
    }
}