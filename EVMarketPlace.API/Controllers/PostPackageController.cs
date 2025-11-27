using System;
using System.Threading.Tasks;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EVMarketPlace.Repositories.RequestDTO;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostPackageController : ControllerBase
    {
        private readonly IPostPackgeService _postPackgeService;
        public PostPackageController(IPostPackgeService postPackgeService)
        {
            _postPackgeService = postPackgeService;
        }
       
        [HttpPost ("create-postpackage")]
        public async Task<IActionResult> Create([FromBody] CreatePostPackageDTO dto)
        {
            var resp = await _postPackgeService.CreatePostPackageAsync(dto);
            return StatusCode(int.Parse(resp.Status ?? "500"), resp);
        }

        [HttpGet("get-by-id/{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var resp = await _postPackgeService.GetPostPackageByIdAsync(id);
            return StatusCode(int.Parse(resp.Status ?? "500"), resp);
        }

        [HttpGet("get-all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var resp = await _postPackgeService.GetAllPostPackagesAsync();
            return Ok(resp);
        }

        [HttpPut("update-postpackage")]
       
        public async Task<IActionResult> Update([FromBody] UpdatePostPackageDTO dto)
        {
            var resp = await _postPackgeService.UpdatePostPackageAsync(dto);
            return StatusCode(int.Parse(resp.Status ?? "500"), resp);
        }

        [HttpDelete("delete/{id:guid}")]
       
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var resp = await _postPackgeService.DeletePostPackageAsync(id);
            return StatusCode(int.Parse(resp.Status ?? "500"), resp);
        }
    }
}
