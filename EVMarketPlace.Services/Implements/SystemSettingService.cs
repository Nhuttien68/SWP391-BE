using Azure;
using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Interfaces;
using System.Security.Claims;

namespace EVMarketPlace.Services.Implements
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly SystemSettingRepository _settingRepository;
        private readonly TransactionRepository _transactionRepository;

        public SystemSettingService(
            SystemSettingRepository settingRepository,
            TransactionRepository transactionRepository)
        {
            _settingRepository = settingRepository;
            _transactionRepository = transactionRepository;
        }


        public async Task<BaseResponse> GetCommissionRateAsync()
        {
            try
            {
                var rate = await _settingRepository.GetCommissionRateAsync();

                return Response(200, "Lấy thông tin hoa hồng thành công.", new
                {
                    CommissionRate = rate,
                    Description = $"Phí hoa hồng hiện tại: {rate}%"
                });
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }
        public async Task<BaseResponse> UpdateCommissionRateAsync(ClaimsPrincipal user, decimal rate)
        {
            try
            {
                var role = GetRole(user);
                if (role != "ADMIN")
                {
                    return Response(403, "Chỉ Admin mới có quyền cập nhật hoa hồng.");
                }

                if (rate < 0 || rate > 100)
                {
                    return Response(400, "Tỷ lệ hoa hồng phải từ 0% đến 100%.");
                }

                var userId = GetUserId(user);
                var success = await _settingRepository.UpdateCommissionRateAsync(rate, userId);

                if (!success)
                {
                    return Response(400, "Không thể cập nhật hoa hồng. Vui lòng thử lại.");
                }

                return Response(200, $"Cập nhật hoa hồng thành công: {rate}%", new
                {
                    CommissionRate = rate,
                    UpdatedBy = userId,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<BaseResponse> GetAllPaymentSettingsAsync(ClaimsPrincipal user)
        {
            try
            {
                var role = GetRole(user);
                if (role != "ADMIN")
                {
                    return Response(403, "Chỉ Admin mới có quyền xem cài đặt.");
                }

                var settings = await _settingRepository.GetSettingsByCategoryAsync("PAYMENT");

                var response = settings.Select(s => new SystemSettingResponse
                {
                    SettingId = s.SettingId,
                    SettingKey = s.SettingKey,
                    SettingValue = s.SettingValue,
                    Description = s.Description,
                    Category = s.Category,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    UpdatedBy = s.UpdatedBy,
                    UpdatedByNavigation = s.UpdatedByNavigation?.FullName
                });

                return Response(200, $"Tổng {settings.Count} cài đặt thanh toán.", response);
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<BaseResponse> GetCommissionReportAsync(ClaimsPrincipal user, DateTime startDate, DateTime endDate)
        {
            try
            {
                var role = GetRole(user);
                if(role != "ADMIN")
                {
                    return Response(403, "Chỉ Admin mới có quyền xem báo cáo hoa hồng.");
                }
                if (startDate > endDate)
                {
                    return Response(400, "Ngày bắt đầu phải trước ngày kết thúc.");
                }

                var allTransactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
                
                // Chỉ tính các transaction có phí hoa hồng (CommissionAmount > 0)
                var transactions = allTransactions
                    .Where(t => t.CommissionAmount.HasValue && t.CommissionAmount.Value > 0)
                    .ToList();

                var totalTransactions = transactions.Count;
                var totalRevenue = transactions.Sum(t => t.Amount ?? 0);
                var totalCommission = transactions.Sum(t => t.CommissionAmount ?? 0);
                var totalSellerReceived = transactions.Sum(t => (t.Amount ?? 0) - (t.CommissionAmount ?? 0));
                var avgCommissionRate = transactions.Any()
                    ? transactions.Average(t => t.CommissionRate ?? 0)
                    : 0;

                return Response(200, "Báo cáo hoa hồng", new
                {
                    Period = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                    TotalTransactions = totalTransactions,
                    TotalRevenue = totalRevenue,
                    TotalCommission = totalCommission,
                    TotalSellerReceived = totalSellerReceived,
                    AverageCommissionRate = Math.Round(avgCommissionRate, 2),
                    CommissionPercentage = totalRevenue > 0
                        ? Math.Round(totalCommission / totalRevenue * 100, 2)
                        : 0
                });
            }
            catch (Exception ex)
            {
                return Response(500, $"Lỗi: {ex.Message}");

            }
        }
        private Guid GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("UserId")?.Value;

            return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
        }

        private string GetRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }

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
