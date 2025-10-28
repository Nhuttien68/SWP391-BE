using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVMarketPlace.Services.Implements
{
    /// <summary>
    /// Dịch vụ quản lý ví người dùng
    /// </summary>
    public class WalletService : IWalletService
    {
        private readonly UserUtility _userUtility;
        private readonly WalletRepository _walletRepository;
        private readonly ILogger<WalletService> _logger;

        public WalletService(
            UserUtility userUtility,
            WalletRepository walletRepository,
            ILogger<WalletService> logger)
        {
            _userUtility = userUtility;
            _walletRepository = walletRepository;
            _logger = logger;
        }

        /// <summary>
        /// Tạo ví mới
        /// </summary>
        public async Task<BaseResponse> CreateWalletAsync()
        {
            try
            {
                var userId = _userUtility.GetUserIdFromToken();
                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("❌ CreateWallet: User not authenticated");
                    return CreateResponse(StatusCodes.Status401Unauthorized, "Người dùng chưa xác thực.");
                }

                var existingWallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (existingWallet != null)
                {
                    _logger.LogWarning("⚠️ CreateWallet: Wallet already exists for UserId={UserId}", userId);
                    return CreateResponse(StatusCodes.Status400BadRequest, "Ví đã tồn tại cho người dùng này.");
                }

                var newWallet = new Wallet
                {
                    WalletId = Guid.NewGuid(),
                    UserId = userId,
                    Balance = 0,
                    LastUpdated = DateTime.UtcNow
                };

                await _walletRepository.CreateAsync(newWallet);

                _logger.LogInformation("✅ CreateWallet: Success for UserId={UserId}, WalletId={WalletId}", userId, newWallet.WalletId);

                return CreateResponse(
                    StatusCodes.Status201Created,
                    "Ví được tạo thành công.",
                    MapToDto(newWallet)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CreateWallet: Error");
                return CreateErrorResponse($"Lỗi tạo ví: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin ví
        /// </summary>
        public async Task<BaseResponse> GetWalletAsync()
        {
            try
            {
                var userId = _userUtility.GetUserIdFromToken();
                if (userId == Guid.Empty)
                    return CreateResponse(StatusCodes.Status401Unauthorized, "Người dùng chưa xác thực.");

                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    _logger.LogWarning("❌ GetWallet: Wallet not found for UserId={UserId}", userId);
                    return CreateResponse(StatusCodes.Status404NotFound, "Không tìm thấy ví. Vui lòng tạo ví trước.");
                }

                _logger.LogInformation("✅ GetWallet: Retrieved for UserId={UserId}", userId);
                return CreateResponse(StatusCodes.Status200OK, "Lấy thông tin ví thành công.", MapToDto(wallet));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GetWallet: Error");
                return CreateErrorResponse($"Lỗi lấy thông tin ví: {ex.Message}");
            }
        }

        /// <summary>
        /// Nạp tiền vào ví (từ VNPay callback)
        /// </summary>
        public async Task<BaseResponse> TopUpWalletAsync(decimal amount, string transactionId, string paymentMethod, Guid userId)
        {
            if (userId == Guid.Empty)
                return CreateResponse(StatusCodes.Status400BadRequest, "UserId không hợp lệ.");

            return await PerformTopUpAsync(userId, amount, transactionId, paymentMethod);
        }

        /// <summary>
        /// Rút tiền từ ví
        /// </summary>
        public async Task<BaseResponse> WithdrawWalletAsync(decimal amount)
        {
            try
            {
                if (amount <= 0)
                    return CreateResponse(StatusCodes.Status400BadRequest, "Số tiền rút phải lớn hơn 0.");

                var userId = _userUtility.GetUserIdFromToken();
                if (userId == Guid.Empty)
                    return CreateResponse(StatusCodes.Status401Unauthorized, "Người dùng chưa xác thực.");

                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                    return CreateResponse(StatusCodes.Status404NotFound, "Không tìm thấy ví.");

                var currentBalance = wallet.Balance ?? 0;
                if (currentBalance < amount)
                {
                    _logger.LogWarning("❌ Withdraw: Insufficient balance. Current={Balance}, Required={Amount}", currentBalance, amount);
                    return CreateResponse(
                        StatusCodes.Status400BadRequest,
                        $"Số dư không đủ. Số dư hiện tại: {currentBalance:N0} VNĐ"
                    );
                }

                var (success, newBalance) = await _walletRepository.TryUpdateBalanceAsync(wallet.WalletId, -amount);
                if (!success)
                    return CreateErrorResponse("Rút tiền thất bại. Vui lòng thử lại.");

                _logger.LogInformation("✅ Withdraw: Success. UserId={UserId}, Amount={Amount}, NewBalance={NewBalance}", userId, amount, newBalance);

                return CreateResponse(StatusCodes.Status200OK, "Rút tiền thành công.", MapToDto(wallet));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Withdraw: Error");
                return CreateErrorResponse($"Lỗi rút tiền: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy số dư hiện tại
        /// </summary>
        public async Task<decimal> GetBalanceAsync()
        {
            try
            {
                var userId = _userUtility.GetUserIdFromToken();
                if (userId == Guid.Empty)
                    return 0;

                return await _walletRepository.GetBalanceAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GetBalance: Error");
                return 0;
            }
        }

        #region Private Helpers

        /// <summary>
        /// Thực hiện nạp tiền (logic chính)
        /// </summary>
        private async Task<BaseResponse> PerformTopUpAsync(Guid userId, decimal amount, string transactionId, string paymentMethod)
        {
            try
            {
                if (amount <= 0)
                    return CreateResponse(StatusCodes.Status400BadRequest, "Số tiền nạp phải lớn hơn 0.");

                if (string.IsNullOrWhiteSpace(transactionId) || string.IsNullOrWhiteSpace(paymentMethod))
                    return CreateResponse(StatusCodes.Status400BadRequest, "Mã giao dịch và phương thức không được để trống.");

                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                    return CreateResponse(StatusCodes.Status404NotFound, "Không tìm thấy ví. Vui lòng tạo ví trước.");

                var oldBalance = wallet.Balance ?? 0;
                var (success, newBalance) = await _walletRepository.TryUpdateBalanceAsync(wallet.WalletId, amount);

                if (!success)
                    return CreateErrorResponse("Cập nhật ví thất bại. Vui lòng thử lại.");

                _logger.LogInformation(
                    "✅ TopUp: Success. WalletId={WalletId}, Amount={Amount}, Old={OldBalance}, New={NewBalance}, TransId={TransactionId}",
                    wallet.WalletId, amount, oldBalance, newBalance, transactionId
                );

                return CreateResponse(
                    StatusCodes.Status200OK,
                    "Nạp tiền thành công. Số dư đã được cập nhật.",
                    new WalletTopUpResponseDto
                    {
                        WalletId = wallet.WalletId,
                        AmountTopUp = amount,
                        OldBalance = oldBalance,
                        NewBalance = newBalance,
                        TopUpDate = DateTime.UtcNow,
                        TransactionId = transactionId,
                        PaymentMethod = paymentMethod
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ PerformTopUp: Error");
                return CreateErrorResponse($"Lỗi nạp tiền: {ex.Message}");
            }
        }

        private static WalletResponseDto MapToDto(Wallet wallet)
        {
            return new WalletResponseDto
            {
                WalletId = wallet.WalletId,
                UserId = wallet.UserId ?? Guid.Empty,
                Balance = wallet.Balance ?? 0,
                LastUpdated = wallet.LastUpdated,
            };
        }

        private static BaseResponse CreateResponse(int statusCode, string message, object? data = null)
        {
            return new BaseResponse
            {
                Status = statusCode.ToString(),
                Message = message,
                Data = data
            };
        }

        private static BaseResponse CreateErrorResponse(string message)
        {
            return new BaseResponse
            {
                Status = StatusCodes.Status500InternalServerError.ToString(),
                Message = message
            };
        }
        public async Task<BaseResponse> DeductAsync(Guid userId, decimal amount)
        {
            try
            {
                if (userId == Guid.Empty)
                    return CreateResponse(StatusCodes.Status400BadRequest, "UserId không hợp lệ.");

                if (amount <= 0)
                    return CreateResponse(StatusCodes.Status400BadRequest, "Số tiền trừ phải lớn hơn 0.");

                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    _logger.LogWarning("❌ Deduct: Wallet not found for UserId={UserId}", userId);
                    return CreateResponse(StatusCodes.Status404NotFound, "Không tìm thấy ví người dùng.");
                }

                var currentBalance = wallet.Balance ?? 0;
                if (currentBalance < amount)
                {
                    _logger.LogWarning("❌ Deduct: Insufficient balance. Current={Balance}, Required={Amount}", currentBalance, amount);
                    return CreateResponse(StatusCodes.Status400BadRequest, $"Số dư không đủ. Hiện có {currentBalance:N0} VNĐ.");
                }

                var (success, newBalance) = await _walletRepository.TryUpdateBalanceAsync(wallet.WalletId, -amount);
                if (!success)
                {
                    _logger.LogWarning("⚠️ Deduct: Failed to update balance for WalletId={WalletId}", wallet.WalletId);
                    return CreateErrorResponse("Trừ tiền thất bại. Vui lòng thử lại.");
                }

                _logger.LogInformation("✅ Deduct: Success. UserId={UserId}, Amount={Amount}, Old={OldBalance}, New={NewBalance}",
                    userId, amount, currentBalance, newBalance);

                return CreateResponse(StatusCodes.Status200OK, "Trừ tiền thành công.", new
                {
                    OldBalance = currentBalance,
                    NewBalance = newBalance,
                    DeductedAmount = amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Deduct: Error");
                return CreateErrorResponse($"Lỗi trừ tiền: {ex.Message}");
            }
        }


        #endregion
    }
}