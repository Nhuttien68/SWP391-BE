using EVMarketPlace.Repositories.RequestDTO.Posts;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{

    ///  Controller CRUD cho Post
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _PostService;
        public PostsController(IPostService postService)
        {
            _PostService = postService;
        }

        [HttpPost("create-post-vehicle")]
        public async Task<IActionResult> CreateVehiclePost([FromForm] PostCreateVehicleRequest request)
        {
            var response = await _PostService.CreateVehiclePostAsync(request);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPost("create-post-battery")]
        public async Task<IActionResult> CreateBatteryPost([FromForm] PostCreateBatteryRequest request)
        {
            var response = await _PostService.CreateBatteryPostAsync(request);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("Get-All-Post")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllPosts()
        {
            var response = await _PostService.GetAllPostsAsync();
            return StatusCode(int.Parse(response.Status), response);
        }
        
        [HttpGet("Get-Post-By-Id/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPostById([FromRoute] Guid id)
        {
            var response = await _PostService.GetPostByIdAsync(id);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpGet("Get-Posts-By-UserId")]
        public async Task<IActionResult> GetPostsByUserId()
        {
            var response = await _PostService.GetPostByUserIdAsync();
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPut("Update-Post-Battery")]
        public async Task<IActionResult> UpdateBatteryPost([FromForm] UpdateBatteryPostRequest request)
        {
            var response = await _PostService.UpdateBatteryPostAsync(request);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPut("Update-Post-Vehicle")]
        public async Task<IActionResult> UpdateVehiclePost([FromForm] UpdateVehiclePostRequest request)
        {
            var response = await _PostService.UpdateVehiclePostAsync(request);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpDelete("Delete-Post/{id}")]
        public async Task<IActionResult> DeletePost([FromRoute] Guid id)
        {
            var response = await _PostService.DeletePostAsync(id);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpGet("Get-All-Post-Pendding")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllPostsPendding()
        {
            var response = await _PostService.GetAllPostWithPendding();
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPut("Approved-Post")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ApprovedPost(Guid postid)
        {
            var response = await _PostService.ApprovedStatus(postid);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpPut("Reject-Post")]
        [Authorize(Roles = "ADMIN")]

        public async Task<IActionResult> RejectPost(Guid postid)
        {
            var response = await _PostService.RejectStatusAsync(postid);
            return StatusCode(int.Parse(response.Status), response);

        }
    }
}

