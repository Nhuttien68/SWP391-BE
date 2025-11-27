using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    // Controller xử lý các API liên quan đến Transaction (Giao dịch)
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        
        //Thanh toán toàn bộ giỏ hàng
        [HttpPost("create-from-cart")]
        public async Task<IActionResult> CreateCartTransaction([FromBody] CreateCartTransactionRequest request)
        {
            var result = await _transactionService.CreateCartTransactionAsync(User, request);
            return StatusCode(int.Parse(result.Status), result);
        }

        // Tạo giao dịch mới (thanh toán)
        [HttpPost("create")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            var response = await _transactionService.CreateTransactionAsync(User, request);
            return StatusCode(int.Parse(response.Status), response);
        }

        // Lấy chi tiết giao dịch theo Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var response = await _transactionService.GetTransactionByIdAsync(User, id);
            return StatusCode(int.Parse(response.Status), response);
        }

        // Lấy danh sách đơn mua của user hiện tại
        [HttpGet("my-purchases")]
        public async Task<IActionResult> GetMyPurchases()
        {
            var response = await _transactionService.GetMyPurchasesAsync(User);
            return StatusCode(int.Parse(response.Status), response);
        }

        // Lấy danh sách đơn bán của user hiện tại
        [HttpGet("my-sales")]
        public async Task<IActionResult> GetMySales()
        {
            var response = await _transactionService.GetMySalesAsync(User);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// Cập nhật trạng thái giao dịch (Seller/Admin)
        //[HttpPut("update-status")]
        //public async Task<IActionResult> UpdateStatus([FromBody] UpdateTransactionStatusRequest request)
        //{
        //    var response = await _transactionService.UpdateTransactionStatusAsync(User, request);
        //    return StatusCode(int.Parse(response.Status), response);
        //}

        // Hủy giao dịch (Buyer/Admin)
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> Cancel([FromRoute] Guid id)
        {
            var response = await _transactionService.CancelTransactionAsync(User, id);
            return StatusCode(int.Parse(response.Status), response);
        }

        // Cập nhật thông tin giao hàng cho đơn đấu giá (Buyer only)
        [HttpPut("{id}/delivery-info")]
        public async Task<IActionResult> UpdateAuctionDeliveryInfo([FromRoute] Guid id, [FromBody] UpdateDeliveryInfoRequest request)
        {
            var response = await _transactionService.UpdateAuctionDeliveryInfoAsync(User, id, request);
            return StatusCode(int.Parse(response.Status), response);
        }

        // Lấy tất cả giao dịch (Admin only)
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var response = await _transactionService.GetAllTransactionsAsync(User);
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}