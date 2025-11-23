using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVMarketPlace.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Tạo ví mới
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateWallet()
        {
            var response = await _walletService.CreateWalletAsync();
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// Lấy thông tin ví
        /// </summary>
        [HttpGet("info")]
        public async Task<IActionResult> GetWallet()
        {
            var response = await _walletService.GetWalletAsync();
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// Rút tiền từ ví
        /// </summary>
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromQuery] decimal amount)
        {
            var response = await _walletService.WithdrawWalletAsync(amount);
            return StatusCode(int.Parse(response.Status), response);
        }

        /// <summary>
        /// Lấy balance hiện tại
        /// </summary>
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var balance = await _walletService.GetBalanceAsync();
            return Ok(new { balance });
        }

        /// <summary>
        /// Lấy lịch sử giao dịch ví
        /// </summary>
        [HttpGet("transaction-history")]
        public async Task<IActionResult> GetTransactionHistory()
        {
            var response = await _walletService.GetWalletTransactionHistoryAsync();
            return StatusCode(int.Parse(response.Status), response);
        }
    }
}