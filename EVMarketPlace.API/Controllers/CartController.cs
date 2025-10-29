using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVMarketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var result = await _cartService.AddToCartAsync(User, request);
            return StatusCode(int.Parse(result.Status), result);
        }

        /// <summary>
        /// Lấy giỏ hàng của user hiện tại
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var result = await _cartService.GetCartByUserAsync(User);
            return StatusCode(int.Parse(result.Status), result);
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemRequest request)
        {
            var result = await _cartService.UpdateCartItemAsync(User, request);
            return StatusCode(int.Parse(result.Status), result);
        }

        /// <summary>
        /// Xóa sản phẩm khỏi giỏ hàng
        /// </summary>
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartRequest request)
        {
            var result = await _cartService.RemoveFromCartAsync(User, request);
            return StatusCode(int.Parse(result.Status), result);
        }

        /// <summary>
        /// Xóa tất cả sản phẩm trong giỏ hàng
        /// </summary>
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var result = await _cartService.ClearCartAsync(User);
            return StatusCode(int.Parse(result.Status), result);
        }
    }
}