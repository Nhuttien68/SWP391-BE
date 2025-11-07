using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.Utils;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EVMarketPlace.Services.Implements
{
    /// <summary>
    /// Dịch vụ xử lý thanh toán VNPay
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly VnPayConfig _config;
        private readonly ILogger<PaymentService> _logger;
        private readonly IWalletService _walletService;
        private readonly UserUtility _userUtility;

        // ✅ Cache để tránh xử lý duplicate orders
        private static readonly HashSet<string> _processedOrders = new();
        private static readonly object _lockObject = new();

        public PaymentService(
            IOptions<VnPayConfig> config,
            ILogger<PaymentService> logger,
            IWalletService walletService,
            UserUtility userUtility)
        {
            _config = config.Value;
            _logger = logger;
            _walletService = walletService;
            _userUtility = userUtility;
        }

        public async Task<BaseResponse> CreatePaymentUrlAsync(HttpContext context, decimal amount, string orderInfo, string? orderId = null)
        {
            try
            {
                // ✅ Validate input
                if (amount <= 0)
                {
                    _logger.LogWarning("❌ CreatePaymentUrl: Invalid amount={Amount}", amount);
                    return CreateBadRequestResponse("Số tiền thanh toán phải lớn hơn 0.");
                }

                // ✅ Get UserId từ token
                Guid userId;
                try
                {
                    userId = _userUtility.GetUserIdFromToken();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ CreatePaymentUrl: Failed to get userId");
                    return CreateUnauthorizedResponse("Không thể lấy thông tin người dùng. Vui lòng đăng nhập lại!");
                }

                // ✅ Tạo OrderId với format: {timestamp}_{userId} để lấy lại userId khi callback
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(_config.TimeZoneId);
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                var generatedOrderId = orderId ?? $"{now.Ticks}_{userId.ToString("N")}"; // Format: 638980123456789_abc123...

                _logger.LogInformation("✅ CreatePaymentUrl: OrderId={OrderId}, UserId={UserId}", generatedOrderId, userId);

                // ✅ Tạo VNPay request
                var vnpay = new VnPayLibrary();
                vnpay.AddRequestData("vnp_Version", _config.Version);
                vnpay.AddRequestData("vnp_Command", _config.Command);
                vnpay.AddRequestData("vnp_TmnCode", _config.TmnCode);
                vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
                vnpay.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", _config.CurrCode);
                vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
                vnpay.AddRequestData("vnp_Locale", _config.Locale);
                vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
                vnpay.AddRequestData("vnp_OrderType", _config.OrderType);
                vnpay.AddRequestData("vnp_ReturnUrl", _config.ReturnUrl);
                vnpay.AddRequestData("vnp_TxnRef", generatedOrderId);
                vnpay.AddRequestData("vnp_ExpireDate", now.AddMinutes(_config.PaymentTimeoutMinutes).ToString("yyyyMMddHHmmss"));

                var paymentUrl = vnpay.CreateRequestUrl(_config.Url, _config.HashSecret);

                _logger.LogInformation("✅ CreatePaymentUrl: Success. Amount={Amount}, OrderId={OrderId}", amount, generatedOrderId);

                return CreateSuccessResponse("Tạo URL thanh toán thành công", new { paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ CreatePaymentUrl: Error");
                return CreateErrorResponse($"Lỗi khi tạo URL thanh toán: {ex.Message}");
            }
        }

        public async Task<BaseResponse> ProcessPaymentReturnAsync(IQueryCollection query, HttpContext context)
        {
            try
            {
                var vnpHash = query["vnp_SecureHash"].ToString();
                var responseCode = query["vnp_ResponseCode"].ToString();
                var orderId = query["vnp_TxnRef"].ToString();
                var transactionId = query["vnp_TransactionNo"].ToString();

                // ✅ Check empty OrderId (duplicate request)
                if (string.IsNullOrEmpty(orderId))
                {
                    _logger.LogWarning("⚠️ ProcessReturn: Empty OrderId - duplicate/invalid request");
                    return CreateBadRequestResponse("OrderId không hợp lệ");
                }

                // ✅ Idempotency: Check if already processed
                lock (_lockObject)
                {
                    if (_processedOrders.Contains(orderId))
                    {
                        _logger.LogWarning("⚠️ ProcessReturn: Duplicate - OrderId={OrderId} already processed", orderId);
                        return CreateSuccessResponse(
                            "Giao dịch đã được xử lý trước đó",
                            new { success = true, orderId, message = "Duplicate request" }
                        );
                    }
                    _processedOrders.Add(orderId);
                }

                _logger.LogInformation("📥 ProcessReturn: OrderId={OrderId}, Code={Code}", orderId, responseCode);

                // ✅ Parse amount
                var amountStr = query["vnp_Amount"].ToString();
                decimal.TryParse(amountStr, out var amount);
                amount /= 100;

                // ✅ Validate signature
                var vnpay = new VnPayLibrary();
                foreach (var (key, value) in query)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_") && key != "vnp_SecureHash")
                    {
                        vnpay.AddResponseData(key, value.ToString());
                    }
                }

                var isValidSignature = vnpay.ValidateSignature(vnpHash, _config.HashSecret);
                if (!isValidSignature)
                {
                    _logger.LogError("❌ ProcessReturn: Invalid signature. OrderId={OrderId}", orderId);
                    return CreateBadRequestResponse("Chữ ký không hợp lệ");
                }

                // ✅ Check response code
                if (responseCode != _config.SuccessResponseCode)
                {
                    _logger.LogWarning("❌ ProcessReturn: Payment failed. Code={Code}, OrderId={OrderId}", responseCode, orderId);
                    return CreateSuccessResponse(
                        GetVnPayErrorMessage(responseCode),
                        new { success = false, orderId, amount }
                    );
                }

                // ✅ Extract UserId from OrderId (format: {timestamp}_{userId})
                Guid userId;
                var orderParts = orderId.Split('_');
                if (orderParts.Length == 2 && Guid.TryParse(orderParts[1], out userId))
                {
                    _logger.LogInformation("✅ ProcessReturn: Extracted UserId={UserId} from OrderId={OrderId}", userId, orderId);
                }
                else
                {
                    _logger.LogError("❌ ProcessReturn: Invalid OrderId format. OrderId={OrderId}", orderId);
                    return CreateBadRequestResponse("OrderId không hợp lệ. Vui lòng thực hiện lại giao dịch.");
                }

                // ✅ TopUp wallet
                var walletResponse = await _walletService.TopUpWalletAsync(amount, transactionId, "VNPAY", userId);

                if (walletResponse.Status == StatusCodes.Status200OK.ToString())
                {
                    _logger.LogInformation("✅ ProcessReturn: Payment + Wallet success. OrderId={OrderId}, Amount={Amount}", orderId, amount);

                    return CreateSuccessResponse(
                        "Thanh toán thành công. Tiền đã được cộng vào ví.",
                        new
                        {
                            success = true,
                            orderId,
                            transactionId,
                            amount,
                            walletUpdate = walletResponse.Data
                        }
                    );
                }

                _logger.LogError("❌ ProcessReturn: Wallet topup failed. Error={Error}", walletResponse.Message);
                return CreateErrorResponse("Thanh toán thành công nhưng cập nhật ví thất bại. Liên hệ hỗ trợ!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ProcessReturn: Error");
                return CreateErrorResponse($"Lỗi xử lý kết quả thanh toán: {ex.Message}");
            }
        }

        #region Helper Methods

        private static BaseResponse CreateSuccessResponse(string message, object? data = null)
        {
            return new BaseResponse
            {
                Status = StatusCodes.Status200OK.ToString(),
                Message = message,
                Data = data
            };
        }

        private static BaseResponse CreateBadRequestResponse(string message)
        {
            return new BaseResponse
            {
                Status = StatusCodes.Status400BadRequest.ToString(),
                Message = message
            };
        }

        private static BaseResponse CreateUnauthorizedResponse(string message)
        {
            return new BaseResponse
            {
                Status = StatusCodes.Status401Unauthorized.ToString(),
                Message = message
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

        private static string GetVnPayErrorMessage(string code) => code switch
        {
            "00" => "Giao dịch thành công",
            "07" => "Trừ tiền thành công nhưng giao dịch bị nghi ngờ",
            "09" => "Thẻ chưa đăng ký dịch vụ InternetBanking",
            "10" => "Xác thực thông tin thẻ sai quá 3 lần",
            "11" => "Hết hạn chờ thanh toán",
            "12" => "Thẻ bị khóa",
            "13" => "Sai mật khẩu OTP",
            "24" => "Khách hủy giao dịch",
            "51" => "Số dư không đủ",
            "65" => "Vượt quá hạn mức giao dịch",
            "75" => "Ngân hàng bảo trì",
            "79" => "Sai mật khẩu thanh toán",
            _ => $"Lỗi: {code}"
        };

        #endregion
    }
}