using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemSettingsController : ControllerBase
    {
        private readonly ISystemSettingService _systemSettingService;
        public SystemSettingsController(ISystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
        }

        // lấy tỉ lệ hoa hồng
        // GET: api/SystemSettings/commission-rate
        [HttpGet("commission-rate")]
        public async Task<IActionResult> GetCommissionRate()
        {
            var response = await _systemSettingService.GetCommissionRateAsync();
            return StatusCode(int.Parse(response.Status), response);
        }
        // cập nhật tỉ lệ hoa hồng
        // PUT: api/SystemSettings/commission-rate
        [HttpPut("commission-rate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateCommissionRate([FromBody] UpdateCommissionRateRequest request)
        {
            var response = await _systemSettingService.UpdateCommissionRateAsync(User, request.CommissionRate);
            return StatusCode(int.Parse(response.Status), response);
        }

        // Admin xem tất cả cài đặt thanh toán
        // GET: api/SystemSettings/payment-settings

        [HttpGet("payment-settings")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetPaymentSettings()
        {
            var response = await _systemSettingService.GetAllPaymentSettingsAsync(User);
            return StatusCode(int.Parse(response.Status), response);
        }

        // Báo cáo hoa hồng theo khoảng thời gian
        [HttpGet("commission-report")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetCommissionReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var response = await _systemSettingService.GetCommissionReportAsync(User, startDate, endDate);
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}
