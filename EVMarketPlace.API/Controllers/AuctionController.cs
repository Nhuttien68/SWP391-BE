using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Services.Implements;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
       
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionRequest req)
        {
            var response = await _auctionService.CreateAuctionAsync(req);
            return StatusCode(int.Parse(response.Status), response);

        }

        // 2️. Đặt giá thầu (Bid)
        [HttpPost("bid")]
        
        public async Task<IActionResult> PlaceBid([FromBody] PlaceBidRequest req)
        {      
                var result = await _auctionService.PlaceBidAsync( req);
                return StatusCode(int.Parse(result.Status), result);
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
