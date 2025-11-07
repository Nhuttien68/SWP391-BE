using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BatteryBrandController : ControllerBase
    {
        private readonly IBatteryBrandService _batteryBrandService;
        public BatteryBrandController(IBatteryBrandService batteryBrandService)
        {
            _batteryBrandService = batteryBrandService;
        }
        [HttpPost("create-battery-brand")]
        public async Task<IActionResult> CreateBatteryBrand([FromBody] BatteryBrandRequestDTO requestDTO)
        {
            var response = await _batteryBrandService.CreateBatteryAsync(requestDTO);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPut("update-battery-brand")]
        public async Task<IActionResult> UpdateBatteryBrand([FromBody] UpdateBatteryBrandRequestDTO requestDTO)
        {
            var response = await _batteryBrandService.UpdateBatteryAsync(requestDTO);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpDelete("delete-battery-brand/{brandBatteryId}")]
        public async Task<IActionResult> DeleteBatteryBrand([FromRoute] Guid brandBatteryId)
        {
            var response = await _batteryBrandService.DeleteBatteryAsync(brandBatteryId);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpGet("get-all-battery-brand")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllBatteryBrand()
        {
            var response = await _batteryBrandService.GetAllBatteryAsync();
            return StatusCode(int.Parse(response.Status), response);
        }
        
        [HttpGet("get-battery-brand-by-id/{brandBatteryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBatteryBrandById([FromRoute] Guid brandBatteryId)
        {
            var response = await _batteryBrandService.GetBatteryByIdAsync(brandBatteryId);
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}