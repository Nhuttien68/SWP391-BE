using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly ILogger<AuctionController> _logger;

        public AuctionController(IAuctionService auctionService, ILogger<AuctionController> logger)
        {
            _auctionService = auctionService;
            _logger = logger;
        }

        // 1️. Tạo phiên đấu giá mới

        [HttpPost("create")]
        [Authorize] 
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionRequest req)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (userIdClaim == null) return Unauthorized("User not authenticated.");

                var userId = Guid.Parse(userIdClaim);
                var result = await _auctionService.CreateAuctionAsync(userId, req);
                return StatusCode(int.Parse(result.Status), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating auction.");
                return StatusCode(500, new { Message = "Internal server error." });
            }
        }

        // 2️. Đặt giá thầu (Bid)
        [HttpPost("bid")]
        [Authorize]
        public async Task<IActionResult> PlaceBid([FromBody] PlaceBidRequest req)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (userIdClaim == null) return Unauthorized("User not authenticated.");

                var userId = Guid.Parse(userIdClaim);
                var result = await _auctionService.PlaceBidAsync(userId, req);
                return StatusCode(int.Parse(result.Status), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error placing bid.");
                return StatusCode(500, new { Message = "Internal server error." });
            }
        }

        // 3️. Lấy chi tiết 1 phiên đấu giá

        [HttpGet("{auctionId}")]
        public async Task<IActionResult> GetAuctionById(Guid auctionId)
        {
            var result = await _auctionService.GetAuctionByIdAsync(auctionId);
            return StatusCode(int.Parse(result.Status), result);
        }

        // 4️. Lấy danh sách các phiên đang hoạt động
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveAuctions()
        {
            var result = await _auctionService.GetActiveAuctionsAsync();
            return Ok(result);
        }

        // 5️. Cập nhật thông tin người nhận sau khi thắng

        [HttpPut("update-transaction/{transactionId}")]
        [Authorize]
        public async Task<IActionResult> UpdateTransactionReceiver(Guid transactionId, [FromBody] UpdateTransactionRequest req)
        {
            var result = await _auctionService.UpdateTransactionReceiverInfoAsync(transactionId, req);
            return StatusCode(int.Parse(result.Status), result);
        }

      
        [HttpPost("close-expired")]
        [Authorize(Roles = "ADMIN")] // chỉ admin được phép
        public async Task<IActionResult> CloseExpiredAuctions()
        {
            var result = await _auctionService.CloseExpiredAuctionsAsync();
            return Ok(result);
        }
    
}
}
