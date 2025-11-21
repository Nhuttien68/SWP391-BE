using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITransactionService _transactionService;
        private readonly IPostService _postService;

        public AdminController(
            IUserService userService,
            ITransactionService transactionService,
            IPostService postService)
        {
            _userService = userService;
            _transactionService = transactionService;
            _postService = postService;
        }


        [HttpGet("users")]
        public async Task<IActionResult> GetAllUser()
        {
            var response = await _userService.GetAllUser();
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("users/count")]
        public async Task<IActionResult> CountUser()
        {
            var response = await _userService.CountUser();
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("transactions/date")]
        public async Task<IActionResult> GetTransactionsByDate([FromQuery] int day, [FromQuery] int month, [FromQuery] int year)
        {
            var response = await _transactionService.GetTransactionsByDateAsync(User, day, month, year);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("transactions/month")]
        public async Task<IActionResult> GetTransactionsByMonth([FromQuery] int month, [FromQuery] int year)
        {
            var response = await _transactionService.GetTransactionsByMonthAsync(User, month, year);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("transactions/year")]
        public async Task<IActionResult> GetTransactionsByYear([FromQuery] int year)
        {
            var response = await _transactionService.GetTransactionsByYearAsync(User, year);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("transactions/range")]
        public async Task<IActionResult> GetTransactionsByRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var response = await _transactionService.GetTransactionsByDateRangeAsync(User, startDate, endDate);
            return StatusCode(int.Parse(response.Status), response);
        }
        [HttpGet("posts/count")]
        public async Task<IActionResult> CountPosts([FromQuery] PostStatusEnum status)
        {
            var response = await _postService.CountPostsByStatusAsync(status);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("posts/count/pending")]
        public async Task<IActionResult> CountPendingPosts()
        {
            var response = await _postService.CountPostsByStatusAsync(PostStatusEnum.PENNDING);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("posts/count/approved")]
        public async Task<IActionResult> CountApprovedPosts()
        {
            var response = await _postService.CountPostsByStatusAsync(PostStatusEnum.APPROVED);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("posts/count/sold")]
        public async Task<IActionResult> CountSoldPosts()
        {
            var response = await _postService.CountPostsByStatusAsync(PostStatusEnum.SOLD);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("posts/count/rejected")]
        public async Task<IActionResult> CountRejectedPosts()
        {
            var response = await _postService.CountPostsByStatusAsync(PostStatusEnum.REJECTED);
            return StatusCode(int.Parse(response.Status), response);
        }


        [HttpGet("posts/date")]
        public async Task<IActionResult> GetPostsByDate(
            [FromQuery] int day,
            [FromQuery] int month,
            [FromQuery] int year,
            [FromQuery] PostStatusEnum status)
        {
            var response = await _postService.GetPostsByDateAndStatusAsync(day, month, year, status);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("posts/month")]
        public async Task<IActionResult> GetPostsByMonth(
            [FromQuery] int month,
            [FromQuery] int year,
            [FromQuery] PostStatusEnum status)
        {
            var response = await _postService.GetPostsByMonthAndStatusAsync(month, year, status);
            return StatusCode(int.Parse(response.Status), response);
        }

        [HttpGet("posts/year")]
        public async Task<IActionResult> GetPostsByYear(
            [FromQuery] int year,
            [FromQuery] PostStatusEnum status)
        {
            var response = await _postService.GetPostsByYearAndStatusAsync(year, status);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// Khóa/Mở khóa user (INACTIVE/ACTIVE)
        /// </summary>
        [HttpPut("users/{userId}/status")]
        public async Task<IActionResult> UpdateUserStatus(
            [FromRoute] Guid userId,
            [FromQuery] string status)
        {
            var response = await _userService.UpdateUserStatusAsync(userId, status);
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}
