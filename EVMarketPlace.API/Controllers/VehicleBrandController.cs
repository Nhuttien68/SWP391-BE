using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VehicleBrandController : ControllerBase
    {
       private readonly IVehicleBrandService _vehicleBrandService;
         public VehicleBrandController(IVehicleBrandService vehicleBrandService)
         {
              _vehicleBrandService = vehicleBrandService;
        }
        [HttpPost("create-vehicle-brand")]
        public async Task<IActionResult> CreateVehicleBrandAsync([FromBody] Repositories.RequestDTO.VehiCleBrandRequestDTO requestDTO)
        {
            var response = await _vehicleBrandService.CreateVehicleBrandAsync(requestDTO);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPut("update-vehicle-brand")]
        public async Task<IActionResult> UpdateVehicleBrandAsync([FromBody] Repositories.RequestDTO.VehiCleBrandUpdateRequestDTO requestDTO)
        {
            var response = await _vehicleBrandService.UpdateVehicleBrandAsync(requestDTO);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpDelete("delete-vehicle-brand/{brandId}")]
        public async Task<IActionResult> DeleteVehicleBrandAsync([FromRoute] Guid brandId)
        {
            var response = await _vehicleBrandService.DeleteVehicleBrandAsync(brandId);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpGet("get-all-vehicle-brands")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllVehicleBrandsAsync()
        {
            var response = await _vehicleBrandService.GetAllVehicleBrandsAsync();
            return StatusCode(int.Parse(response.Status), response);
        }
        
        [HttpGet("get-vehicle-brand-by-id/{brandId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetVehicleBrandByIdAsync([FromRoute] Guid brandId)
        {
            var response = await _vehicleBrandService.GetVehicleBrandByIdAsync(brandId);
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}
