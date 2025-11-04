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
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        [HttpPost("Crate-review-for-post")]
        public async Task<IActionResult> CreateReviewForPostAsync([FromBody] ReviewCreateDTO dto)
        {
            var response = await _reviewService.CreateReviewForPostAsync(dto);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPost("Crate-review-for-Seller")]
        public async Task<IActionResult> CreateReviewForSellerAsync([FromBody] ReviewCreateDTO dto)
        {
            var response = await _reviewService.CreateReviewForSellerAsync(dto);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpDelete("Delete-Review/{id}")]
        public async Task<IActionResult> DeleteReviewAsync([FromRoute] Guid id)
        {
            var response = await _reviewService.DeleteReviewAsync(id);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpGet("Get-Reviews-By-PostId/{postId}")]
        public async Task<IActionResult> GetByPostIdAsync([FromRoute] Guid postId)
        {
            var response = await _reviewService.GetByPostIdAsync(postId);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpGet("Get-Reviews-By-UserId/{userId}")]
        public async Task<IActionResult> GetByUserIdAsync([FromRoute] Guid userId)
        {
            var response = await _reviewService.GetByUserIdAsync(userId);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPut("Update-Review")]
        public async Task<IActionResult> UpdateReviewAsync([FromBody] UpdateReviewDTO dto)
        {
            var response = await _reviewService.UpdateReviewAsync(dto);
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}
