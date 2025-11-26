using EVMarketPlace.Repositories.Entity;
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
        private readonly CartRepository _cartRepository;
        private readonly CartItemRepository _cartItemRepository;
        private readonly IWalletService _walletService;
        private readonly SystemSettingRepository _systemSettingRepository;
        private readonly WalletRepository _walletRepository;
        private readonly WalletTransactionRepository _walletTransactionRepository;
        private readonly UserRepository _userRepository;

        public TransactionService(
            TransactionRepository transactionRepository,
            PostRepository postRepository,
            CartRepository cartRepository,
            CartItemRepository cartItemRepository,
            IWalletService walletService,
            SystemSettingRepository systemSettingRepository,
            WalletRepository walletRepository,
            WalletTransactionRepository walletTransactionRepository,
            UserRepository userRepository)
        {
            _transactionRepository = transactionRepository;
            _postRepository = postRepository;
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _walletService = walletService;
            _systemSettingRepository = systemSettingRepository;
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _userRepository = userRepository;
        }



        //  HÀM TÍNH HOA HỒNG (MỀM - LẤY TỪ DATABASE)
        private async Task<(decimal rate, decimal amount, decimal sellerReceived)> CalculateCommissionAsync(decimal totalAmount)
        {
            // Lấy tỷ lệ hoa hồng từ SystemSettings
            var rate = await _systemSettingRepository.GetCommissionRateAsync();

            // Tính toán
            var commissionAmount = Math.Round(totalAmount * rate / 100, 2);
            var sellerReceived = totalAmount - commissionAmount;

            return (rate, commissionAmount, sellerReceived);
        }

        //THANH TOÁN GIỎ HÀNG
        public async Task<BaseResponse> CreateCartTransactionAsync(ClaimsPrincipal user, CreateCartTransactionRequest request)
        {
            try
            {
                var userId = GetUserId(user);
                if (userId == Guid.Empty)
                {
                    return Response(401, "Người dùng chưa xác thực.");
                }

                // validate payment để tránh lỗi
                if (!Enum.TryParse<PaymentMethodEnum>(request.PaymentMethod, true, out _))
                {
                    return Response(400, "Phương thức thanh toán không hợp lệ.");
                }

                // Lấy giỏ hàng
                var cart = await _cartRepository.GetCartByIdAsync(request.CartId);
                if (cart == null)
                {
                    return Response(404, "Giỏ hàng không tồn tại.");
                }

                // Kiểm tra quyền sở hữu giỏ hàng
                if (cart.UserId != userId)
                {
                    return Response(403, "Bạn không có quyền thanh toán giỏ hàng này.");
                }

                // Kiểm tra giỏ hàng rỗng
                if (cart.CartItems == null || !cart.CartItems.Any())
                {
                    return Response(400, "Giỏ hàng trống, không thể thanh toán.");
                }

                // Tính tổng tiền và validate từng sản phẩm
                decimal totalAmount = 0;
                var validItems = new List<CartItem>();

                foreach (var item in cart.CartItems)
                {
                    var post = await _postRepository.GetByIdAsync(item.PostId.Value);

                    if (post == null)
                    {
                        return Response(400, $"Sản phẩm không tồn tại.");
                    }

                    // Kiểm tra không mua sản phẩm của chính mình
                    if (post.UserId == userId)
                    {
                        return Response(400, $"Giỏ hàng chứa sản phẩm của bạn: {post.Title}");
                    }

                    // Kiểm tra trạng thái sản phẩm
                    if (post.Status != PostStatusEnum.APPROVED.ToString())
                    {
                        return Response(400, $"Sản phẩm '{post.Title}' không còn khả dụng (Status: {post.Status}).");
                    }

                    totalAmount += (post.Price ?? 0);
                    validItems.Add(item);
                }

                // CHỈ XỬ LÝ VÍ KHI DÙNG PAYMENT METHOD = WALLET
                if (request.PaymentMethod == "WALLET")
                {
                    // Kiểm tra số dư
                    var buyerBalance = await _walletService.GetBalanceAsync();
                    if (buyerBalance < totalAmount)
                    {
                        return Response(400, $"Số dư không đủ. Cần {totalAmount:N0} VNĐ, hiện có {buyerBalance:N0} VNĐ.");
                    }

                    var cartTransactionId = Guid.NewGuid().ToString();


                    // Trừ tiền người mua
                    var deductResult = await _walletService.DeductAsync(userId, totalAmount, cartTransactionId);
                    if (int.Parse(deductResult.Status) != 200)
                    {
                        return Response(400, "Không thể trừ tiền từ ví. Vui lòng thử lại.");
                    }

                    // Track các seller đã nhận tiền để rollback nếu cần
                    var paidSellers = new List<(Guid SellerId, decimal Amount, string TransactionId)>();
                    bool paymentFailed = false;



                    foreach (var item in validItems)
                    {
                        var post = await _postRepository.GetPostByIdWithImageAsync(item.PostId.Value);

                        // var seller recived để lấy số tiền sau khi trừ hoa hồng
                        var (_, _, sellerReceived) = await CalculateCommissionAsync(post.Price ?? 0);

                        var itemTransactionId = Guid.NewGuid().ToString();

                        var productTitle = string.IsNullOrWhiteSpace(post.Title) || post.Title.Equals("string", StringComparison.OrdinalIgnoreCase)
                            ? $"Sản phẩm #{post.PostId.ToString().Substring(0, 8)}"
                            : post.Title;

                        var topUpResult = await _walletService.AddSalesRevenueAsync(
                            sellerId: post.UserId,
                            amount: sellerReceived,
                            transactionId: itemTransactionId,
                            postTitle: productTitle //  Sử dụng giá trị đã xử lý
                        );

                        if (int.Parse(topUpResult.Status) != 200)
                        {
                            paymentFailed = true;
                            break;
                        }

                        paidSellers.Add((post.UserId, sellerReceived, itemTransactionId));
                        // CỘNG HOA HỒNG VÀO TÀI KHOẢN ADMIN
                        var commissionSuccess = await CreditCommissionToAdminAsync( // hàm riêng để cộng hoa hồng
                            commissionAmount: (post.Price ?? 0) - sellerReceived, // hoa hồng là phần chênh lệch
                            transactionId: itemTransactionId, // dùng chung transactionId với seller để dễ tracking
                            description: $"Hoa hồng từ giao dịch - Sản phẩm: {productTitle}"
                        );

                        if (!commissionSuccess) // nếu cộng hoa hồng thất bại
                        {
                            paymentFailed = true; // đánh dấu thất bại
                            break;
                        }
                    }
                    // Nếu có lỗi, rollback TOÀN BỘ
                    if (paymentFailed)
                    {
                        // Lấy lại tiền từ các seller đã nhận
                        foreach (var (sellerId, amount, transId) in paidSellers)
                        {
                            await _walletService.DeductAsync(sellerId, amount, $"REFUND-{transId}");
                        }

                        // Hoàn tiền lại cho buyer
                        await _walletService.TopUpWalletAsync(
                            totalAmount,
                            $"REFUND-{cartTransactionId}", // tạo mã giao dịch hoàn tiền mới
                            "REFUND",
                            userId
                        );

                        return Response(400, "Không thể chuyển tiền cho người bán. Giao dịch đã được hoàn tác toàn bộ.");
                    }
                }

                // Tạo transaction cho từng sản phẩm
                var transactions = new List<Transaction>();
                decimal totalCommission = 0;
                decimal totalSellerReceived = 0;

                foreach (var item in validItems)
                {

                    var post = await _postRepository.GetPostByIdAsync(item.PostId.Value); // lấy lại post để tạo 
                    if (post == null) continue; // bỏ qua nếu post không tồn tại
                    var (commissionRate, commissionAmount, sellerReceived) =
                        await CalculateCommissionAsync(post.Price ?? 0); // tính hoa hồng cho từng sản phẩm

                    totalCommission += commissionAmount; // cộng dồn hoa hồng 
                    totalSellerReceived += sellerReceived; // cộng dồn số tiền seller nhận được

                    // Cập nhật trạng thái sản phẩm thành SOLD
                    try
                    {
                        await _postRepository.UpdateStatusSoldAsync(item.PostId.Value);
                    }
                    catch { }

                    // Tạo transaction
                    var transaction = new Transaction
                    {
                        TransactionId = Guid.NewGuid(),
                        BuyerId = userId,
                        SellerId = post.UserId,
                        PostId = item.PostId,
                        CartId = request.CartId,
                        Amount = post.Price,
                        CommissionRate = commissionRate,
                        CommissionAmount = commissionAmount,
                        SellerReceived = sellerReceived,
                        PaymentMethod = request.PaymentMethod,
                        Status = TransactionStatusEnum.COMPLETED.ToString(),
                        CreatedAt = DateTime.UtcNow,
                        ReceiverName = request.ReceiverName,
                        ReceiverPhone = request.ReceiverPhone,
                        ReceiverAddress = request.ReceiverAddress,
                        Note = request.Note
                    };

                    try
                    {
                        await _transactionRepository.CreateAsync(transaction);
                        transactions.Add(transaction);
                    }
                    catch (Exception ex)
                    {
                        return Response(500, $"Lỗi lưu giao dịch: {ex.InnerException?.Message ?? ex.Message}");
                    }
                }


                // Xóa giỏ hàng sau khi thanh toán thành công
                foreach (var item in validItems)
                {
                    await _cartItemRepository.DeleteCartItemAsync(item.CartItemId);
                }

                return Response(201, $"Thanh toán thành công {transactions.Count} sản phẩm từ giỏ hàng. Tổng: {totalAmount:N0} VNĐ", new
                {
                    TotalAmount = totalAmount,
                    TotalCommission = totalCommission,
                    TotalSellerReceived = totalSellerReceived,
                    TotalItems = transactions.Count,
                    CartId = request.CartId,
                    Transactions = transactions.Select(t => new
                    {
                        t.TransactionId,
                        t.PostId,
                        t.Amount,
                        t.CommissionRate,
                        t.CommissionAmount,
                        t.SellerReceived,
                        t.Status
                    })
                });
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        // Tạo giao dịch mới - Thanh toán và hoàn thành ngay lập tức
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
                        var s when s == PostStatusEnum.PENDING.ToString() => "Sản phẩm đang chờ duyệt.",
                        var s when s == PostStatusEnum.REJECTED.ToString() => "Sản phẩm đã bị từ chối.",
                        var s when s == PostStatusEnum.SOLD.ToString() => "Sản phẩm đã được bán.",
                        var s when s == PostStatusEnum.DELETED.ToString() => "Sản phẩm đã bị xóa.",
                        _ => "Sản phẩm không có sẵn."
                    };
                    return Response(400, statusMessage);
                }

                // tính hoa hồng từ database
                var (commissionRate, commissionAmount, sellerReceived) =
                    await CalculateCommissionAsync(post.Price ?? 0);

                // TẠO TRANSACTION ID TRƯỚC
                var transactionId = Guid.NewGuid();

                var transaction = new Transaction
                {
                    TransactionId = transactionId,
                    BuyerId = userId,
                    SellerId = post.UserId,
                    PostId = request.PostId,
                    Amount = post.Price,
                    CommissionRate = commissionRate,
                    CommissionAmount = commissionAmount,
                    SellerReceived = sellerReceived,
                    PaymentMethod = request.PaymentMethod,
                    Status = TransactionStatusEnum.COMPLETED.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    ReceiverName = request.ReceiverName,
                    ReceiverPhone = request.ReceiverPhone,
                    ReceiverAddress = request.ReceiverAddress,
                    Note = request.Note
                };

                // Kiểm tra số dư ví người mua (CHỈ KHI DÙNG WALLET)
                if (request.PaymentMethod == "WALLET")
                {
                    var buyerBalance = await _walletService.GetBalanceAsync();
                    if (buyerBalance < post.Price)
                    {
                        return Response(400, $"Số dư không đủ. Cần {post.Price:N0} VNĐ, hiện có {buyerBalance:N0} VNĐ.");
                    }

                    // Trừ tiền người mua
                    var deductResult = await _walletService.DeductAsync(userId, post.Price ?? 0, transactionId.ToString());
                    if (int.Parse(deductResult.Status) != 200)
                    {
                        return Response(400, "Không thể trừ tiền từ ví. Vui lòng thử lại.");
                    }

                    //CẬP NHẬT TRẠNG THÁI SẢN PHẨM TRƯỚC khi cộng tiền
                    try
                    {
                        await _postRepository.UpdateStatusSoldAsync(request.PostId);
                    }
                    catch (Exception ex)
                    {
                        await _walletService.TopUpWalletAsync(
                            post.Price ?? 0,
                            $"REFUND-{Guid.NewGuid()}",
                            "REFUND",
                            userId
                        );
                        return Response(400, $"Cập nhật sản phẩm thất bại: {ex.InnerException?.Message ?? ex.Message}");
                    }

                    // CỘNG TIỀN CHO SELLER với TransactionId đã tạo
                    var topUpResult = await _walletService.AddSalesRevenueAsync(
                        sellerId: post.UserId,
                        amount: sellerReceived,
                        transactionId: transactionId.ToString(),
                        postTitle: post.Title ?? "Sản phẩm"
                    );

                    if (int.Parse(topUpResult.Status) != 200)
                    {
                        // Rollback: Đặt lại post thành APPROVED
                        try
                        {
                            await _postRepository.UpdateStatusApprovedAsync(request.PostId);
                        }
                        catch { }

                        // Hoàn tiền cho buyer
                        await _walletService.TopUpWalletAsync(
                            post.Price ?? 0,
                            $"REFUND-{Guid.NewGuid()}",
                            "REFUND",
                            userId
                        );
                        return Response(400, "Không thể chuyển tiền cho người bán. Giao dịch đã được hoàn tác.");
                    }

                    //CỘNG HOA HỒNG VÀO TÀI KHOẢN ADMIN
                    var commissionSuccess = await CreditCommissionToAdminAsync( // hàm riêng để cộng hoa hồng
                        commissionAmount, // hoa hồng là phần chênh lệch 
                        transactionId.ToString(), // dùng chung transactionId với seller để dễ tracking
                        $"Hoa hồng từ giao dịch - Sản phẩm: {post.Title ?? "N/A"}"
                    );

                    if (!commissionSuccess) // nếu cộng hoa hồng thất bại
                    {
                        // Rollback: Trừ lại tiền từ seller
                        await _walletService.DeductAsync(post.UserId, sellerReceived, $"ROLLBACK-{transactionId}"); // tạo mã giao dịch rollback

                        // Rollback: Đặt lại post thành APPROVED
                        try
                        {
                            await _postRepository.UpdateStatusApprovedAsync(request.PostId);
                        }
                        catch { }

                        // Hoàn tiền cho buyer
                        await _walletService.TopUpWalletAsync(
                            post.Price ?? 0,
                            $"REFUND-{Guid.NewGuid()}",
                            "REFUND",
                            userId
                        );

                        return Response(400, "Không thể xử lý hoa hồng. Giao dịch đã được hoàn tác.");
                    }

                }
                else
                {

                    try
                    {
                        await _postRepository.UpdateStatusSoldAsync(request.PostId);
                    }
                    catch (Exception ex)
                    {
                        return Response(400, $"Cập nhật sản phẩm thất bại: {ex.InnerException?.Message ?? ex.Message}");
                    }
                }

                // LƯU TRANSACTION VÀO DATABASE
                try
                {
                    await _transactionRepository.CreateAsync(transaction);
                }
                catch (Exception ex)
                {
                    return Response(500, $"Lỗi lưu giao dịch: {ex.InnerException?.Message ?? ex.Message}");
                }

                var response = await MapToDTO(transaction);
                return Response(201,
                    $"Thanh toán thành công. Phí hoa hồng {commissionRate}% = {commissionAmount:N0} VNĐ. Người bán nhận: {sellerReceived:N0} VNĐ",
                    response);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
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

        //// Cập nhật trạng thái giao dịch
        //public async Task<BaseResponse> UpdateTransactionStatusAsync(ClaimsPrincipal user, UpdateTransactionStatusRequest request)
        //{
        //    try
        //    {
        //        var userId = GetUserId(user);
        //        if (userId == Guid.Empty)
        //        {
        //            return Response(401, "Người dùng chưa xác thực.");
        //        }

        //        // Validate status
        //        if (!Enum.TryParse<TransactionStatusEnum>(request.Status, true, out _))
        //        {
        //            return Response(400, "Trạng thái không hợp lệ.");
        //        }

        //        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);
        //        if (transaction == null)
        //        {
        //            return Response(404, "Giao dịch không tồn tại.");
        //        }

        //        // Chỉ seller hoặc admin có thể cập nhật
        //        var role = GetRole(user);
        //        if (role != "ADMIN" && transaction.SellerId != userId)
        //        {
        //            return Response(403, "Chỉ người bán hoặc Admin mới có thể cập nhật.");
        //        }

        //        // Kiểm tra trạng thái hiện tại
        //        if (transaction.Status == TransactionStatusEnum.COMPLETED.ToString())
        //        {
        //            return Response(400, "Giao dịch đã hoàn thành, không thể cập nhật.");
        //        }

        //        if (transaction.Status == TransactionStatusEnum.CANCELLED.ToString())
        //        {
        //            return Response(400, "Giao dịch đã hủy, không thể cập nhật.");
        //        }

        //        // Cập nhật trạng thái
        //        transaction.Status = request.Status;

        //        // Nếu hoàn thành → cộng tiền cho seller
        //        if (request.Status == TransactionStatusEnum.COMPLETED.ToString())
        //        {
        //            // thay vì gọi GetByIdAsync
        //            var post = await _postRepository.GetPostByIdAsync(transaction.PostId.Value);

        //            if (post != null)
        //            {
        //                post.Status = PostStatusEnum.SOLD.ToString();
        //                await _postRepository.ForceUpdateAsync(post);
        //            }


        //            // Cộng tiền vào ví người bán
        //            var topUpResult = await _walletService.TopUpWalletAsync(
        //                transaction.Amount ?? 0,
        //                transaction.TransactionId.ToString(),
        //                "TRANSACTION",
        //                transaction.SellerId ?? Guid.Empty
        //            );

        //            if (int.Parse(topUpResult.Status) != 200)
        //            {
        //                return Response(400, "Không thể cộng tiền cho người bán. Vui lòng thử lại.");
        //            }
        //        }

        //        await _transactionRepository.UpdateAsync(transaction);

        //        var response = await MapToDTO(transaction);

        //        // Thông báo cụ thể theo từng status
        //        var message = request.Status switch
        //        {
        //            var s when s == TransactionStatusEnum.COMPLETED.ToString() =>
        //                "Giao dịch hoàn thành. Tiền đã được chuyển cho người bán.",
        //            var s when s == TransactionStatusEnum.PENDING.ToString() =>
        //                "Đơn hàng đang chờ xử lý.",
        //            _ => "Cập nhật trạng thái thành công."
        //        };

        //        return Response(200, message, response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Response(500, $"Lỗi: {ex.Message}");
        //    }
        //}

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
                if (transaction.Status == TransactionStatusEnum.CANCELLED.ToString())
                {
                    return Response(400, "Giao dịch đã được hủy trước đó.");
                }

                // Hoàn tiền cho người mua
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

                // Trừ tiền từ người bán
                var deductResult = await _walletService.DeductAsync(
                    transaction.SellerId ?? Guid.Empty,
                    transaction.Amount ?? 0,
                    $"CANCEL-{transaction.TransactionId}" // Cung cấp ID để ghi log
                );
                if (int.Parse(deductResult.Status) != 200)
                {
                    return Response(400, "Không thể trừ tiền từ người bán. Vui lòng liên hệ hỗ trợ.");
                }

                // Mở khóa sản phẩm
                var post = await _postRepository.GetByIdAsync(transaction.PostId.Value);
                if (post != null && post.Status == PostStatusEnum.SOLD.ToString())
                {
                    post.Status = PostStatusEnum.APPROVED.ToString();
                    await _postRepository.UpdateAsync(post);
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

        // Cộng phí hoa hồng vào tài khoản Admin
        private async Task<bool> CreditCommissionToAdminAsync(decimal commissionAmount, string transactionId, string description)
        {
            try
            {
                // 1. Lấy admin user (Role = ADMIN)
                var adminUser = await _userRepository.GetAdminUserAsync();
                if (adminUser == null)
                {
                    return false;
                }

                // 2. Lấy ví admin
                var adminWallet = await _walletRepository.GetWalletByUserIdAsync(adminUser.UserId);
                if (adminWallet == null)
                {
                    return false;
                }

                // 3. Cộng tiền vào ví admin
                var walletResult = await _walletRepository.TryUpdateBalanceAsync(adminWallet.WalletId, commissionAmount);
                if (!walletResult.Success)
                {
                    return false;
                }

                // 4. Tạo giao dịch WalletTransaction cho admin
                var adminTransaction = new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    WalletId = adminWallet.WalletId,
                    TransactionType = "COMMISSION_FEE",
                    Amount = commissionAmount,
                    BalanceBefore = adminWallet.Balance,
                    BalanceAfter = adminWallet.Balance + commissionAmount,
                    ReferenceId = transactionId,
                    PaymentMethod = "WALLET",
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };
                await _walletTransactionRepository.CreateAsync(adminTransaction); // lưu giao dịch ví

                return true;
            }
            catch
            {
                return false;
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
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value // lấy UserId từ ClaimTypes.NameIdentifier
                     ?? user.FindFirst("UserId")?.Value; // hoặc từ claim "UserId" nếu không có

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
                PostImageUrl = t.Post?.PostImages?.FirstOrDefault()?.ImageUrl,

                // Thông tin người bán
                SellerId = t.SellerId,
                SellerName = t.Seller?.FullName ?? "N/A",
                SellerEmail = t.Seller?.Email,

                // Thông tin người mua
                BuyerId = t.BuyerId,
                BuyerName = t.Buyer?.FullName ?? "N/A",
                BuyerEmail = t.Buyer?.Email
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
        // Thống kê giao dịch theo ngày
        public async Task<BaseResponse> GetTransactionsByDateAsync(ClaimsPrincipal user, int day, int month, int year)
        {
            try
            {
                var role = GetRole(user);
                if (role != "ADMIN")
                    return Response(403, "Chỉ Admin mới có quyền xem giao dịch theo ngày.");

                var transactions = await _transactionRepository.GetByDateAsync(day, month, year);

                var list = transactions.Select(MapToListItemDTO).ToList();

                return Response(200, $"Tìm thấy {list.Count} giao dịch trong ngày {day}/{month}/{year}.", list);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }
        // Thống kê giao dịch theo tháng
        public async Task<BaseResponse> GetTransactionsByMonthAsync(ClaimsPrincipal user, int month, int year)
        {
            try
            {
                var role = GetRole(user);
                if (role != "ADMIN")
                    return Response(403, "Chỉ Admin mới có quyền xem giao dịch theo tháng.");

                var transactions = await _transactionRepository.GetByMonthAsync(month, year);

                var list = transactions.Select(MapToListItemDTO).ToList();

                return Response(200, $"Tìm thấy {list.Count} giao dịch trong tháng {month}/{year}.", list);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }
        // Thống kê giao dịch theo năm
        public async Task<BaseResponse> GetTransactionsByYearAsync(ClaimsPrincipal user, int year)
        {
            try
            {
                var role = GetRole(user);
                if (role != "ADMIN")
                    return Response(403, "Chỉ Admin mới có quyền xem giao dịch theo năm.");

                var transactions = await _transactionRepository.GetByYearAsync(year);

                var list = transactions.Select(MapToListItemDTO).ToList();

                return Response(200, $"Tìm thấy {list.Count} giao dịch trong năm {year}.", list);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }
        // Thống kê giao dịch theo khoảng thời gian
        public async Task<BaseResponse> GetTransactionsByDateRangeAsync(ClaimsPrincipal user, DateTime startDate, DateTime endDate)
        {
            try
            {
                var role = GetRole(user);
                if (role != "ADMIN")
                    return Response(403, "Chỉ Admin mới có quyền xem giao dịch theo khoảng thời gian.");

                if (startDate > endDate)
                    return Response(400, "Ngày bắt đầu không được lớn hơn ngày kết thúc.");

                var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);

                var list = transactions.Select(MapToListItemDTO).ToList();

                return Response(200, $"Tìm thấy {list.Count} giao dịch từ {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}.", list);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }
    }
}