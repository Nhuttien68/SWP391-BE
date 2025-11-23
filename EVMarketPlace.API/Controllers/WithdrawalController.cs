using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WithdrawalController : ControllerBase
    {
        private readonly IWithdrawalService _withdrawalService;

        public WithdrawalController(IWithdrawalService withdrawalService)
        {
            _withdrawalService = withdrawalService;
        }

        /// <summary>
        /// User tạo yêu cầu rút tiền
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateWithdrawalRequest([FromBody] CreateWithdrawalRequest request)
        {
            var response = await _withdrawalService.CreateWithdrawalRequestAsync(User, request);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// User xem yêu cầu rút tiền của mình
        /// </summary>
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyWithdrawalRequests()
        {
            var response = await _withdrawalService.GetMyWithdrawalRequestsAsync(User);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// Admin xem tất cả yêu cầu rút tiền
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllWithdrawalRequests()
        {
            var response = await _withdrawalService.GetAllWithdrawalRequestsAsync(User);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// Admin duyệt yêu cầu rút tiền
        /// </summary>
        [HttpPut("approve/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ApproveWithdrawal([FromRoute] Guid id, [FromBody] ProcessWithdrawalRequest? request)
        {
            var response = await _withdrawalService.ApproveWithdrawalAsync(User, id, request?.AdminNote);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// Admin từ chối yêu cầu rút tiền
        /// </summary>
        [HttpPut("reject/{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RejectWithdrawal([FromRoute] Guid id, [FromBody] ProcessWithdrawalRequest request)
        {
            var response = await _withdrawalService.RejectWithdrawalAsync(User, id, request.AdminNote ?? "Không đủ điều kiện");
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}