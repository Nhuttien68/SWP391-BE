using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EVMarketPlace.Services.Implements
{
    /// <summary>
    /// Service xử lý nghiệp vụ rút tiền từ ví người dùng
    /// Bao gồm: tạo yêu cầu rút tiền, xem danh sách yêu cầu, admin duyệt/từ chối
    /// </summary>
    public class WithdrawalService : IWithdrawalService
    {
        #region Fields

        private readonly WithdrawalRequestRepository _withdrawalRepository;
        private readonly WalletRepository _walletRepository;
        private readonly WalletTransactionRepository _walletTransactionRepository;
        private readonly UserUtility _userUtility;
        private readonly ILogger<WithdrawalService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo WithdrawalService với các dependencies cần thiết
        /// </summary>
        public WithdrawalService(
            WithdrawalRequestRepository withdrawalRepository,
            WalletRepository walletRepository,
            WalletTransactionRepository walletTransactionRepository,
            UserUtility userUtility,
            ILogger<WithdrawalService> logger)
        {
            _withdrawalRepository = withdrawalRepository;
            _walletRepository = walletRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _userUtility = userUtility;
            _logger = logger;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tạo yêu cầu rút tiền từ ví
        /// Luồng xử lý:
        /// 1. Xác thực user
        /// 2. Validate số tiền rút (phải > 0)
        /// 3. Kiểm tra ví tồn tại và số dư đủ
        /// 4. Tạo yêu cầu rút tiền với status = PENDING
        /// 5. Chờ admin duyệt
        public async Task<BaseResponse> CreateWithdrawalRequestAsync(ClaimsPrincipal user, CreateWithdrawalRequest request)
        {
            try
            {
                // Lấy UserId từ JWT token
                var userId = GetUserId(user);
                if (userId == Guid.Empty)
                    return CreateResponse(401, "Người dùng chưa xác thực.");

                // Validate số tiền rút
                if (request.Amount <= 0)
                    return CreateResponse(400, "Số tiền rút phải lớn hơn 0.");

                // Kiểm tra ví tồn tại
                var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                    return CreateResponse(404, "Không tìm thấy ví.");

                // Kiểm tra số dư
                var currentBalance = wallet.Balance ?? 0;
                if (currentBalance < request.Amount)
                    return CreateResponse(400, $"Số dư không đủ. Hiện có: {currentBalance:N0} VNĐ");

                // Tạo yêu cầu rút tiền với status PENDING
                var withdrawal = new WithdrawalRequest
                {
                    WithdrawalId = Guid.NewGuid(),
                    UserId = userId,
                    WalletId = wallet.WalletId,
                    Amount = request.Amount,
                    BankName = request.BankName,
                    BankAccountNumber = request.BankAccountNumber,
                    BankAccountName = request.BankAccountName,
                    Status = "PENDING",
                    RequestedAt = DateTime.UtcNow,
                    Note = request.Note
                };

                await _withdrawalRepository.CreateAsync(withdrawal);

                _logger.LogInformation("✅ Withdrawal request created: UserId={UserId}, Amount={Amount}", userId, request.Amount);

                return CreateResponse(201, "Yêu cầu rút tiền đã được gửi. Vui lòng chờ admin duyệt.", new
                {
                    withdrawal.WithdrawalId,
                    withdrawal.Amount,
                    withdrawal.Status,
                    withdrawal.RequestedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CreateWithdrawalRequest: Error");
                return CreateResponse(500, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// User xem danh sách yêu cầu rút tiền của chính mình
        /// </summary>
        public async Task<BaseResponse> GetMyWithdrawalRequestsAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = GetUserId(user);
                if (userId == Guid.Empty)
                    return CreateResponse(401, "Người dùng chưa xác thực.");

                // Lấy danh sách yêu cầu rút tiền của user (có include navigation properties)
                var requests = await _withdrawalRepository.GetByUserIdAsync(userId);

                // Map sang DTO để trả về client
                var response = requests.Select(w => new
                {
                    w.WithdrawalId,
                    w.Amount,
                    w.BankName,
                    w.BankAccountNumber,
                    w.BankAccountName,
                    w.Status,
                    w.RequestedAt,
                    w.ProcessedAt,
                    ProcessedByName = w.ProcessedByNavigation?.FullName,
                    w.Note,
                    w.AdminNote
                }).ToList();

                return CreateResponse(200, $"Tìm thấy {response.Count} yêu cầu rút tiền.", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GetMyWithdrawalRequests: Error");
                return CreateResponse(500, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin xem tất cả yêu cầu rút tiền trong hệ thống
        /// </summary>

        public async Task<BaseResponse> GetAllWithdrawalRequestsAsync(ClaimsPrincipal user)
        {
            try
            {
                // Kiểm tra quyền admin
                var role = GetRole(user);
                if (role != "ADMIN")
                    return CreateResponse(403, "Chỉ Admin mới có quyền xem tất cả yêu cầu.");

                // Lấy tất cả yêu cầu rút tiền (có include navigation properties)
                var requests = await _withdrawalRepository.GetAllWithIncludeAsync();

                // Map sang DTO với đầy đủ thông tin (bao gồm thông tin user và admin duyệt)
                var response = requests.Select(w => new
                {
                    w.WithdrawalId,
                    w.UserId,
                    UserName = w.User?.FullName,
                    UserEmail = w.User?.Email,
                    w.Amount,
                    w.BankName,
                    w.BankAccountNumber,
                    w.BankAccountName,
                    w.Status,
                    w.RequestedAt,
                    w.ProcessedAt,
                    ProcessedByName = w.ProcessedByNavigation?.FullName,
                    w.Note,
                    w.AdminNote
                }).ToList();

                return CreateResponse(200, $"Tìm thấy {response.Count} yêu cầu rút tiền.", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GetAllWithdrawalRequests: Error");
                return CreateResponse(500, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Admin duyệt yêu cầu rút tiền
        /// </summary>
        /// <param name="user">Thông tin admin từ JWT token</param>
        /// <param name="withdrawalId">Id yêu cầu rút tiền cần duyệt</param>
        /// <param name="adminNote">Ghi chú từ admin (optional)</param>
        /// <returns>BaseResponse với kết quả duyệt</returns>
        /// <remarks>
        /// Luồng xử lý:
        /// 1. Kiểm tra quyền admin
        /// 2. Validate yêu cầu (phải ở status PENDING)
        /// 3. Kiểm tra số dư ví
        /// 4. Trừ tiền từ ví
        /// 5. Log lịch sử giao dịch vào WalletTransactions
        /// 6. Cập nhật status = APPROVED
        /// </remarks>
        public async Task<BaseResponse> ApproveWithdrawalAsync(ClaimsPrincipal user, Guid withdrawalId, string? adminNote)
        {
            try
            {
                // Lấy thông tin admin
                var adminId = GetUserId(user);
                var role = GetRole(user);
                if (role != "ADMIN")
                    return CreateResponse(403, "Chỉ Admin mới có quyền duyệt.");

                // Lấy yêu cầu rút tiền (có include navigation properties để tránh null reference)
                var withdrawal = await _withdrawalRepository.GetByIdWithIncludeAsync(withdrawalId);
                if (withdrawal == null)
                    return CreateResponse(404, "Yêu cầu rút tiền không tồn tại.");

                // Kiểm tra trạng thái (chỉ duyệt được yêu cầu PENDING)
                if (withdrawal.Status != "PENDING")
                    return CreateResponse(400, $"Yêu cầu đã được xử lý trước đó (Status: {withdrawal.Status})");

                // Validate WalletId
                if (!withdrawal.WalletId.HasValue)
                    return CreateResponse(400, "WalletId không hợp lệ.");

                // Lấy ví
                var wallet = await _walletRepository.GetWalletByIdAsync(withdrawal.WalletId.Value);
                if (wallet == null)
                    return CreateResponse(404, "Không tìm thấy ví.");

                // Kiểm tra số dư
                var currentBalance = wallet.Balance ?? 0;
                if (currentBalance < withdrawal.Amount)
                    return CreateResponse(400, $"Số dư không đủ. Hiện có: {currentBalance:N0} VNĐ, yêu cầu: {withdrawal.Amount:N0} VNĐ");

                // Trừ tiền từ ví (atomic operation)
                var (success, newBalance) = await _walletRepository.TryUpdateBalanceAsync(
                    withdrawal.WalletId.Value,
                    -(withdrawal.Amount ?? 0)
                );

                if (!success)
                    return CreateResponse(500, "Không thể trừ tiền từ ví. Vui lòng thử lại.");

                // Log lịch sử giao dịch vào WalletTransactions
                await _walletTransactionRepository.CreateLogAsync(new WalletTransaction
                {
                    WalletTransactionId = Guid.NewGuid(),
                    WalletId = withdrawal.WalletId.Value,
                    TransactionType = "WITHDRAW",
                    Amount = -(withdrawal.Amount ?? 0),
                    BalanceBefore = currentBalance,
                    BalanceAfter = newBalance,
                    ReferenceId = withdrawalId.ToString(),
                    PaymentMethod = "BANK",
                    Description = $"Rút tiền về {withdrawal.BankName} - {withdrawal.BankAccountNumber}",
                    CreatedAt = DateTime.UtcNow
                });

                // Cập nhật trạng thái yêu cầu
                withdrawal.Status = "APPROVED";
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.ProcessedBy = adminId;
                withdrawal.AdminNote = adminNote;
                await _withdrawalRepository.UpdateAsync(withdrawal);

                _logger.LogInformation("✅ Withdrawal approved: WithdrawalId={WithdrawalId}, AdminId={AdminId}, Amount={Amount}",
                    withdrawalId, adminId, withdrawal.Amount);

                return CreateResponse(200, "Duyệt rút tiền thành công. Tiền đã được trừ khỏi ví.", new
                {
                    withdrawal.WithdrawalId,
                    withdrawal.Status,
                    withdrawal.Amount,
                    OldBalance = currentBalance,
                    NewBalance = newBalance,
                    ProcessedAt = withdrawal.ProcessedAt,
                    ProcessedBy = adminId,
                    AdminNote = withdrawal.AdminNote
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ApproveWithdrawal: Error - WithdrawalId={WithdrawalId}", withdrawalId);
                return CreateResponse(500, $"Lỗi: {ex.Message}");
            }
        }


        /// Admin từ chối yêu cầu rút tiền
        /// <param name="user">Thông tin admin từ JWT token</param>
        /// <param name="withdrawalId">Id yêu cầu rút tiền cần từ chối</param>
        /// <param name="adminNote">Lý do từ chối (bắt buộc)</param>
        /// Khi từ chối:
        /// - Không trừ tiền từ ví
        /// - Cập nhật status = REJECTED
        /// - Lưu lý do từ chối vào AdminNote
        public async Task<BaseResponse> RejectWithdrawalAsync(ClaimsPrincipal user, Guid withdrawalId, string adminNote)
        {
            try
            {
                // Lấy thông tin admin
                var adminId = GetUserId(user);
                var role = GetRole(user);
                if (role != "ADMIN")
                    return CreateResponse(403, "Chỉ Admin mới có quyền từ chối.");

                // Lấy yêu cầu rút tiền (có include navigation properties)
                var withdrawal = await _withdrawalRepository.GetByIdWithIncludeAsync(withdrawalId);
                if (withdrawal == null)
                    return CreateResponse(404, "Yêu cầu rút tiền không tồn tại.");

                // Kiểm tra trạng thái (chỉ từ chối được yêu cầu PENDING)
                if (withdrawal.Status != "PENDING")
                    return CreateResponse(400, $"Yêu cầu đã được xử lý trước đó (Status: {withdrawal.Status})");

                // Cập nhật trạng thái sang REJECTED (không trừ tiền)
                withdrawal.Status = "REJECTED";
                withdrawal.ProcessedAt = DateTime.UtcNow;
                withdrawal.ProcessedBy = adminId;
                withdrawal.AdminNote = adminNote;
                await _withdrawalRepository.UpdateAsync(withdrawal);

                _logger.LogInformation(" Withdrawal rejected: WithdrawalId={WithdrawalId}, AdminId={AdminId}, Reason={Reason}",
                    withdrawalId, adminId, adminNote);

                return CreateResponse(200, "Từ chối yêu cầu rút tiền thành công.", new
                {
                    withdrawal.WithdrawalId,
                    withdrawal.Status,
                    withdrawal.AdminNote
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ RejectWithdrawal: Error - WithdrawalId={WithdrawalId}", withdrawalId);
                return CreateResponse(500, $"Lỗi: {ex.Message}");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Lấy UserId từ JWT claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal từ JWT token</param>
        /// <returns>UserId dạng Guid, hoặc Guid.Empty nếu không tìm thấy</returns>
        private Guid GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("UserId")?.Value;
            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        /// <summary>
        /// Lấy Role từ JWT claims
        /// </summary>
        /// <param name="user">ClaimsPrincipal từ JWT token</param>
        /// <returns>Role name (ADMIN, USER, ...) hoặc empty string nếu không tìm thấy</returns>
        private string GetRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Tạo BaseResponse với status code, message và data
        /// </summary>
        /// <param name="statusCode">HTTP status code (200, 400, 401, 403, 404, 500, ...)</param>
        /// <param name="message">Message trả về client</param>
        /// <param name="data">Data trả về (optional)</param>
        /// <returns>BaseResponse để trả về controller</returns>
        private BaseResponse CreateResponse(int statusCode, string message, object? data = null)
        {
            return new BaseResponse
            {
                Status = statusCode.ToString(),
                Message = message,
                Data = data
            };
        }

        #endregion
    }
}