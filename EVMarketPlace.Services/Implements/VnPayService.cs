using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Repositories.ResponseDTO.Payments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EVMarketPlace.Services.Implements
{
    public class VnPayService
    {
        private readonly VnPayConfig _config;
        private readonly ILogger<VnPayService> _logger;

        public VnPayService(IOptions<VnPayConfig> config, ILogger<VnPayService> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public BaseResponse CreatePaymentUrl(HttpContext context, decimal amount, string orderInfo, string? orderId = null)
        {
            try
            {
                var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_config.TimeZoneId);
                var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);

                var pay = new VnPayLibrary();

                pay.AddRequestData("vnp_Version", _config.Version);
                pay.AddRequestData("vnp_Command", _config.Command);
                pay.AddRequestData("vnp_TmnCode", _config.TmnCode);
                pay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
                pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
                pay.AddRequestData("vnp_CurrCode", _config.CurrCode);
                pay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
                pay.AddRequestData("vnp_Locale", _config.Locale);
                pay.AddRequestData("vnp_OrderInfo", orderInfo);
                pay.AddRequestData("vnp_OrderType", _config.OrderType);
                pay.AddRequestData("vnp_ReturnUrl", _config.ReturnUrl);
                pay.AddRequestData("vnp_TxnRef", orderId ?? timeNow.Ticks.ToString());
                pay.AddRequestData("vnp_ExpireDate", timeNow.AddMinutes(_config.PaymentTimeoutMinutes).ToString("yyyyMMddHHmmss"));

                var paymentUrl = pay.CreateRequestUrl(_config.Url, _config.HashSecret);

                return new BaseResponse
                {
                    Status = StatusCodes.Status200OK.ToString(),
                    Message = "Tạo URL thanh toán thành công",
                    Data = new { paymentUrl }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment URL");
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Lỗi khi tạo URL thanh toán: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Xử lý return từ VNPay - Return BaseResponse để Controller xử lý
        /// </summary>
        public BaseResponse ProcessPaymentReturn(IQueryCollection query)
        {
            try
            {
                var pay = new VnPayLibrary();

                foreach (var (key, value) in query)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(key, value.ToString());
                    }
                }

                var vnpSecureHash = query["vnp_SecureHash"].ToString();
                var responseCode = query["vnp_ResponseCode"].ToString();
                var isValidSignature = pay.ValidateSignature(vnpSecureHash, _config.HashSecret);

                _logger.LogInformation("VNPay Return - OrderId: {OrderId}, ResponseCode: {Code}, SignatureValid: {Valid}",
                    query["vnp_TxnRef"], responseCode, isValidSignature);

                //  Parse amount để trả về frontend
                var amountStr = query["vnp_Amount"].ToString();
                decimal amount = 0;
                if (!string.IsNullOrEmpty(amountStr) && decimal.TryParse(amountStr, out var parsedAmount))
                {
                    amount = parsedAmount / 100; // VNPay trả về amount * 100
                }

                var response = new VnPaymentResponseModel
                {
                    Success = responseCode == _config.SuccessResponseCode,
                    PaymentMethod = "VNPAY",
                    OrderDescription = query["vnp_OrderInfo"].ToString(),
                    OrderId = query["vnp_TxnRef"].ToString(),
                    PaymentId = query["vnp_TransactionNo"].ToString(),
                    TransactionId = query["vnp_TransactionNo"].ToString(),
                    Token = vnpSecureHash,
                    VnPayResponseCode = responseCode
                };

                //  Return 200 OK với đầy đủ thông tin cho frontend
                if (responseCode == _config.SuccessResponseCode)
                {
                    if (!isValidSignature)
                    {
                        _logger.LogWarning("⚠️ Payment successful but signature invalid for order: {OrderId}", response.OrderId);
                    }

                    return new BaseResponse
                    {
                        Status = StatusCodes.Status200OK.ToString(),
                        Message = "Thanh toán thành công",
                        Data = new
                        {
                            success = true,
                            orderId = response.OrderId,
                            transactionId = response.TransactionId,
                            amount = amount,
                            paymentMethod = response.PaymentMethod,
                            orderDescription = response.OrderDescription,
                            responseCode = response.VnPayResponseCode,
                            message = response.ResponseCodeMessage
                        }
                    };
                }
                else
                {
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status200OK.ToString(),
                        Message = response.ResponseCodeMessage,
                        Data = new
                        {
                            success = false,
                            orderId = response.OrderId,
                            amount = amount,
                            responseCode = response.VnPayResponseCode,
                            message = response.ResponseCodeMessage
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay return");
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Lỗi xử lý kết quả thanh toán: {ex.Message}",
                    Data = new
                    {
                        success = false,
                        message = ex.Message
                    }
                };
            }
        }

        public BaseResponse ProcessIpnCallback(IQueryCollection query)
        {
            try
            {
                var pay = new VnPayLibrary();

                foreach (var (key, value) in query)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(key, value.ToString());
                    }
                }

                var vnpSecureHash = query["vnp_SecureHash"].ToString();
                var responseCode = query["vnp_ResponseCode"].ToString();
                var isValidSignature = pay.ValidateSignature(vnpSecureHash, _config.HashSecret);

                var orderId = query["vnp_TxnRef"].ToString();
                var amountStr = query["vnp_Amount"].ToString();
                var transactionNo = query["vnp_TransactionNo"].ToString();

                decimal amount = 0;
                if (decimal.TryParse(amountStr, out var parsedAmount))
                {
                    amount = parsedAmount / 100;
                }

                _logger.LogInformation("VNPay IPN - Order: {OrderId}, Code: {Code}, Signature: {Valid}",
                    orderId, responseCode, isValidSignature);

                // Validate signature cho IPN 
                if (!isValidSignature)
                {
                    _logger.LogError("❌ Invalid IPN signature for order: {OrderId}", orderId);
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status400BadRequest.ToString(),
                        Message = "Chữ ký không hợp lệ",
                        Data = new { RspCode = "97", Message = "Invalid Signature" }
                    };
                }

                var isSuccess = responseCode == _config.SuccessResponseCode;

                if (isSuccess)
                {
                    _logger.LogInformation("✅ IPN Success - Order: {OrderId}, Amount: {Amount}", orderId, amount);

                    

                    return new BaseResponse
                    {
                        Status = StatusCodes.Status200OK.ToString(),
                        Message = "Xác nhận thanh toán thành công",
                        Data = new
                        {
                            RspCode = "00",
                            Message = "Confirm Success",
                            OrderId = orderId,
                            Amount = amount,
                            TransactionNo = transactionNo
                        }
                    };
                }
                else
                {
                    _logger.LogWarning("❌ IPN Failed - Code: {Code}, Order: {OrderId}", responseCode, orderId);
                    return new BaseResponse
                    {
                        Status = StatusCodes.Status200OK.ToString(),
                        Message = "Giao dịch thất bại",
                        Data = new { RspCode = responseCode, Message = "Transaction Failed" }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing IPN");
                return new BaseResponse
                {
                    Status = StatusCodes.Status500InternalServerError.ToString(),
                    Message = $"Lỗi xử lý IPN: {ex.Message}",
                    Data = new { RspCode = "99", Message = "System Error" }
                };
            }
        }
    }
}