using EVMarketPlace.Repositories.Entity;
using EVMarketPlace.Repositories.Enum;
using EVMarketPlace.Repositories.Enums;
using EVMarketPlace.Repositories.Repository;
using EVMarketPlace.Repositories.RequestDTO;
using EVMarketPlace.Repositories.ResponseDTO;
using EVMarketPlace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EVMarketPlace.Services.Implements
{
    /// <summary>
    /// Service xử lý nghiệp vụ giỏ hàng (Shopping Cart)
    /// Bao gồm: thêm, xem, cập nhật, xóa sản phẩm trong giỏ hàng
    /// </summary>
    public class CartService : ICartService
    {
        private readonly CartRepository _cartRepository;
        private readonly CartItemRepository _cartItemRepository;
        private readonly PostRepository _postRepository;

        public CartService(
            CartRepository cartRepository,
            CartItemRepository cartItemRepository,
            PostRepository postRepository)
        {
            _cartRepository = cartRepository;
            _cartItemRepository = cartItemRepository;
            _postRepository = postRepository;
        }

        #region Public Methods

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng
        /// - Kiểm tra quyền: User không thể mua sản phẩm của chính mình
        /// - Kiểm tra trạng thái: Chỉ thêm được sản phẩm đã APPROVED
        /// - Nếu sản phẩm đã có trong giỏ: cộng thêm số lượng
        /// - Nếu chưa có: tạo mới cart item
        /// </summary>
        /// <param name="user">ClaimsPrincipal chứa thông tin user đăng nhập</param>
        /// <param name="request">Request chứa PostId và Quantity</param>
        /// <returns>BaseResponse với cart đã cập nhật</returns>
        public async Task<BaseResponse> AddToCartAsync(ClaimsPrincipal user, AddToCartRequest request)
        {
            try
            {
                // Xác thực user từ JWT token
                var userId = GetUserIdFromClaims(user);
                if (userId == Guid.Empty)
                {
                    return CreateResponse(StatusCodes.Status401Unauthorized, "Người dùng chưa xác thực.");
                }

                // Validate PostId hợp lệ
                if (request.PostId == Guid.Empty)
                {
                    return CreateResponse(StatusCodes.Status400BadRequest, "PostId không hợp lệ.");
                }

                // Lấy thông tin sản phẩm từ database
                var post = await _postRepository.GetByIdAsync(request.PostId);
                if (post == null)
                {
                    return CreateResponse(StatusCodes.Status404NotFound, "Sản phẩm không tồn tại.");
                }

                // Kiểm tra user không được mua sản phẩm của chính mình
                if (post.UserId == userId)
                {
                    return CreateResponse(StatusCodes.Status400BadRequest, "Bạn không thể thêm sản phẩm của chính mình vào giỏ hàng.");
                }

                // Kiểm tra trạng thái sản phẩm (chỉ cho phép APPROVED)
                var statusValidation = ValidatePostStatus(post.Status);
                if (!statusValidation.IsValid)
                {
                    return CreateResponse(StatusCodes.Status400BadRequest, statusValidation.Message);
                }

                // Lấy hoặc tạo mới giỏ hàng cho user
                var cart = await GetOrCreateCartAsync(userId);

                // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
                var existingCartItem = await _cartItemRepository.GetCartItemByPostAsync(cart.CartId, request.PostId);

                if (existingCartItem != null)
                {
                    // Sản phẩm đã có -> cộng thêm số lượng
                    await UpdateExistingCartItemQuantityAsync(existingCartItem, request.Quantity);
                }
                else
                {
                    // Sản phẩm chưa có -> thêm mới vào giỏ hàng
                    await AddNewCartItemAsync(cart.CartId, request.PostId, request.Quantity);
                }

                // Lấy lại cart đã cập nhật và trả về response
                var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
                var cartResponse = MapToCartResponseDTO(updatedCart);

                return CreateResponse(StatusCodes.Status200OK, "Thêm vào giỏ hàng thành công.", cartResponse);
            }
            catch (Exception ex)
            {
                return CreateResponse(StatusCodes.Status500InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy giỏ hàng của user hiện tại
        /// - Trả về giỏ hàng rỗng nếu user chưa có cart hoặc cart không có item
        /// - Trả về chi tiết giỏ hàng bao gồm: danh sách sản phẩm, tổng tiền
        /// </summary>
        /// <param name="user">ClaimsPrincipal chứa thông tin user đăng nhập</param>
        /// <returns>BaseResponse với thông tin giỏ hàng</returns>
        public async Task<BaseResponse> GetCartByUserAsync(ClaimsPrincipal user)
        {
            try
            {
                // Xác thực user từ JWT token
                var userId = GetUserIdFromClaims(user);
                if (userId == Guid.Empty)
                {
                    return CreateResponse(StatusCodes.Status401Unauthorized, "Người dùng chưa xác thực.");
                }

                // Lấy giỏ hàng từ database
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);

                // Kiểm tra giỏ hàng trống
                if (IsCartEmpty(cart))
                {
                    return CreateResponse(StatusCodes.Status200OK, "Giỏ hàng trống.", CreateEmptyCartResponse(userId));
                }

                // Map entity sang DTO và trả về
                var cartResponse = MapToCartResponseDTO(cart);
                return CreateResponse(StatusCodes.Status200OK, "Lấy giỏ hàng thành công.", cartResponse);
            }
            catch (Exception ex)
            {
                return CreateResponse(StatusCodes.Status500InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật số lượng sản phẩm trong giỏ hàng
        /// - Kiểm tra quyền: User chỉ được cập nhật cart của chính mình
        /// - Cập nhật số lượng mới cho cart item
        /// </summary>
        /// <param name="user">ClaimsPrincipal chứa thông tin user đăng nhập</param>
        /// <param name="request">Request chứa CartItemId và Quantity mới</param>
        /// <returns>BaseResponse với cart đã cập nhật</returns>
        public async Task<BaseResponse> UpdateCartItemAsync(ClaimsPrincipal user, UpdateCartItemRequest request)
        {
            try
            {
                // Xác thực user từ JWT token
                var userId = GetUserIdFromClaims(user);
                if (userId == Guid.Empty)
                {
                    return CreateResponse(StatusCodes.Status401Unauthorized, "Người dùng chưa xác thực.");
                }

                // Validate CartItemId hợp lệ
                if (request.CartItemId == Guid.Empty)
                {
                    return CreateResponse(StatusCodes.Status400BadRequest, "CartItemId không hợp lệ.");
                }

                // Lấy cart item từ database
                var cartItem = await _cartItemRepository.GetCartItemByIdAsync(request.CartItemId);
                if (cartItem == null)
                {
                    return CreateResponse(StatusCodes.Status404NotFound, "Sản phẩm không tồn tại trong giỏ hàng.");
                }

                // Kiểm tra quyền sở hữu cart
                var ownershipValidation = await ValidateCartOwnershipAsync(cartItem.CartId.Value, userId);
                if (!ownershipValidation.IsValid)
                {
                    return CreateResponse(StatusCodes.Status403Forbidden, ownershipValidation.Message);
                }

                // Cập nhật số lượng mới
                cartItem.Quantity = request.Quantity;
                await _cartItemRepository.UpdateCartItemAsync(cartItem);

                // Lấy lại cart đã cập nhật và trả về
                var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
                var cartResponse = MapToCartResponseDTO(updatedCart);

                return CreateResponse(StatusCodes.Status200OK, "Cập nhật giỏ hàng thành công.", cartResponse);
            }
            catch (Exception ex)
            {
                return CreateResponse(StatusCodes.Status500InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa một sản phẩm khỏi giỏ hàng
        /// - Kiểm tra quyền: User chỉ được xóa item trong cart của chính mình
        /// - Xóa cart item khỏi database
        /// </summary>
        /// <param name="user">ClaimsPrincipal chứa thông tin user đăng nhập</param>
        /// <param name="request">Request chứa CartItemId cần xóa</param>
        /// <returns>BaseResponse với cart đã cập nhật</returns>
        public async Task<BaseResponse> RemoveFromCartAsync(ClaimsPrincipal user, RemoveFromCartRequest request)
        {
            try
            {
                // Xác thực user từ JWT token
                var userId = GetUserIdFromClaims(user);
                if (userId == Guid.Empty)
                {
                    return CreateResponse(StatusCodes.Status401Unauthorized, "Người dùng chưa xác thực.");
                }

                // Validate CartItemId hợp lệ
                if (request.CartItemId == Guid.Empty)
                {
                    return CreateResponse(StatusCodes.Status400BadRequest, "CartItemId không hợp lệ.");
                }

                // Lấy cart item từ database
                var cartItem = await _cartItemRepository.GetCartItemByIdAsync(request.CartItemId);
                if (cartItem == null)
                {
                    return CreateResponse(StatusCodes.Status404NotFound, "Sản phẩm không tồn tại trong giỏ hàng.");
                }

                // Kiểm tra quyền sở hữu cart
                var ownershipValidation = await ValidateCartOwnershipAsync(cartItem.CartId.Value, userId);
                if (!ownershipValidation.IsValid)
                {
                    return CreateResponse(StatusCodes.Status403Forbidden, ownershipValidation.Message);
                }

                // Xóa cart item khỏi database
                await _cartItemRepository.DeleteCartItemAsync(request.CartItemId);

                // Lấy lại cart đã cập nhật và trả về
                var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
                var cartResponse = updatedCart != null ? MapToCartResponseDTO(updatedCart) : null;

                return CreateResponse(StatusCodes.Status200OK, "Xóa sản phẩm khỏi giỏ hàng thành công.", cartResponse);
            }
            catch (Exception ex)
            {
                return CreateResponse(StatusCodes.Status500InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa tất cả sản phẩm trong giỏ hàng
        /// - Xóa toàn bộ cart items của user
        /// - Giữ lại cart (không xóa cart entity)
        /// </summary>
        /// <param name="user">ClaimsPrincipal chứa thông tin user đăng nhập</param>
        /// <returns>BaseResponse thông báo kết quả</returns>
        public async Task<BaseResponse> ClearCartAsync(ClaimsPrincipal user)
        {
            try
            {
                // Xác thực user từ JWT token
                var userId = GetUserIdFromClaims(user);
                if (userId == Guid.Empty)
                {
                    return CreateResponse(StatusCodes.Status401Unauthorized, "Người dùng chưa xác thực.");
                }

                // Lấy giỏ hàng từ database
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    return CreateResponse(StatusCodes.Status404NotFound, "Giỏ hàng không tồn tại.");
                }

                // Xóa tất cả cart items
                if (cart.CartItems != null && cart.CartItems.Any())
                {
                    foreach (var item in cart.CartItems.ToList())
                    {
                        await _cartItemRepository.DeleteCartItemAsync(item.CartItemId);
                    }
                }

                return CreateResponse(StatusCodes.Status200OK, "Xóa tất cả sản phẩm khỏi giỏ hàng thành công.");
            }
            catch (Exception ex)
            {
                return CreateResponse(StatusCodes.Status500InternalServerError, $"Lỗi: {ex.Message}");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Lấy UserId từ ClaimsPrincipal (hỗ trợ nhiều loại claim types)
        /// Thứ tự ưu tiên: NameIdentifier > UserId > sub > nameid
        /// </summary>
        /// <param name="user">ClaimsPrincipal từ JWT token</param>
        /// <returns>UserId dạng Guid, hoặc Guid.Empty nếu không tìm thấy</returns>
        private Guid GetUserIdFromClaims(ClaimsPrincipal user)
        {
            // Thử lấy từ các claim types khác nhau (fallback chain)
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst("UserId")?.Value
                           ?? user.FindFirst("sub")?.Value
                           ?? user.FindFirst("nameid")?.Value;

            // Validate và parse sang Guid
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Guid.Empty;
            }

            return userId;
        }

        /// <summary>
        /// Kiểm tra trạng thái sản phẩm có hợp lệ để thêm vào giỏ hàng không
        /// Chỉ cho phép sản phẩm có status = APPROVED
        /// </summary>
        /// <param name="status">Trạng thái hiện tại của post</param>
        /// <returns>ValidationResult với IsValid và Message</returns>
        private ValidationResult ValidatePostStatus(string? status)
        {
            // Chỉ cho phép thêm sản phẩm đã được duyệt (APPROVED)
            if (status != PostStatusEnum.APPROVED.ToString())
            {
                // Trả về message cụ thể theo từng trạng thái
                var message = status switch
                {
                    var s when s == PostStatusEnum.PENNDING.ToString() => "Sản phẩm đang chờ duyệt và chưa thể mua.",
                    var s when s == PostStatusEnum.REJECTED.ToString() => "Sản phẩm đã bị từ chối và không thể mua.",
                    var s when s == PostStatusEnum.DELETED.ToString() => "Sản phẩm đã bị xóa và không thể mua.",
                    _ => "Sản phẩm không có sẵn để thêm vào giỏ hàng."
                };
                return new ValidationResult { IsValid = false, Message = message };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Kiểm tra quyền sở hữu giỏ hàng
        /// Đảm bảo user chỉ có thể thao tác với cart của chính mình
        /// </summary>
        /// <param name="cartId">Id của cart cần kiểm tra</param>
        /// <param name="userId">Id của user hiện tại</param>
        /// <returns>ValidationResult với IsValid và Message</returns>
        private async Task<ValidationResult> ValidateCartOwnershipAsync(Guid cartId, Guid userId)
        {
            var cart = await _cartRepository.GetCartByIdAsync(cartId);

            // Cart không tồn tại hoặc không thuộc về user
            if (cart?.UserId != userId)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Bạn không có quyền thao tác với giỏ hàng này."
                };
            }

            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Kiểm tra giỏ hàng có rỗng không
        /// </summary>
        /// <param name="cart">ShoppingCart entity</param>
        /// <returns>True nếu cart null hoặc không có items</returns>
        private bool IsCartEmpty(ShoppingCart? cart)
        {
            return cart == null || cart.CartItems == null || !cart.CartItems.Any();
        }

        /// <summary>
        /// Lấy hoặc tạo mới giỏ hàng cho user
        /// - Nếu user đã có cart -> trả về cart hiện tại
        /// - Nếu chưa có -> tạo mới cart với status ACTIVE
        /// </summary>
        /// <param name="userId">Id của user</param>
        /// <returns>ShoppingCart entity</returns>
        private async Task<ShoppingCart> GetOrCreateCartAsync(Guid userId)
        {
            // Kiểm tra xem user đã có cart chưa
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart != null)
            {
                return cart;
            }

            // Tạo mới cart với status ACTIVE
            var newCart = new ShoppingCart
            {
                CartId = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = CartStatusEnum.ACTIVE.ToString()
            };

            return await _cartRepository.CreateCartAsync(newCart);
        }

        /// <summary>
        /// Cập nhật số lượng cho cart item đã tồn tại
        /// Cộng thêm quantity vào số lượng hiện tại
        /// </summary>
        /// <param name="cartItem">Cart item cần cập nhật</param>
        /// <param name="additionalQuantity">Số lượng cần thêm</param>
        private async Task UpdateExistingCartItemQuantityAsync(CartItem cartItem, int additionalQuantity)
        {
            cartItem.Quantity = (cartItem.Quantity ?? 0) + additionalQuantity;
            await _cartItemRepository.UpdateCartItemAsync(cartItem);
        }

        /// <summary>
        /// Thêm mới cart item vào giỏ hàng
        /// </summary>
        /// <param name="cartId">Id của cart</param>
        /// <param name="postId">Id của post (sản phẩm)</param>
        /// <param name="quantity">Số lượng</param>
        private async Task AddNewCartItemAsync(Guid cartId, Guid postId, int quantity)
        {
            var cartItem = new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cartId,
                PostId = postId,
                Quantity = quantity,
                AddedAt = DateTime.UtcNow
            };

            await _cartItemRepository.CreateCartItemAsync(cartItem);
        }

        /// <summary>
        /// Tạo response cho giỏ hàng rỗng
        /// </summary>
        /// <param name="userId">Id của user</param>
        /// <returns>CartResponseDTO với cart rỗng</returns>
        private CartResponseDTO CreateEmptyCartResponse(Guid userId)
        {
            return new CartResponseDTO
            {
                CartId = Guid.Empty,
                UserId = userId,
                Status = CartStatusEnum.ACTIVE.ToString(),
                CartItems = new List<CartItemResponseDTO>(),
                TotalAmount = 0
            };
        }

        /// <summary>
        /// Map ShoppingCart entity sang CartResponseDTO
        /// Bao gồm: thông tin cart, danh sách items, tổng tiền
        /// </summary>
        /// <param name="cart">ShoppingCart entity</param>
        /// <returns>CartResponseDTO để trả về client</returns>
        private CartResponseDTO MapToCartResponseDTO(ShoppingCart? cart)
        {
            // Handle null cart
            if (cart == null)
            {
                return new CartResponseDTO
                {
                    CartItems = new List<CartItemResponseDTO>(),
                    TotalAmount = 0
                };
            }

            // Map cart items sang DTO
            var cartItems = cart.CartItems?.Select(ci => new CartItemResponseDTO
            {
                CartItemId = ci.CartItemId,
                PostId = ci.PostId ?? Guid.Empty,
                PostTitle = ci.Post?.Title ?? "N/A",
                PostDescription = ci.Post?.Description,
                Price = ci.Post?.Price ?? 0,
                Quantity = ci.Quantity ?? 1,
                Subtotal = (ci.Post?.Price ?? 0) * (ci.Quantity ?? 1),
                AddedAt = ci.AddedAt ?? DateTime.UtcNow,
                ImageUrl = ci.Post?.PostImages?.FirstOrDefault()?.ImageUrl
            }).ToList() ?? new List<CartItemResponseDTO>();

            // Tính tổng tiền toàn bộ giỏ hàng
            var totalAmount = cartItems.Sum(ci => ci.Subtotal ?? 0);

            return new CartResponseDTO
            {
                CartId = cart.CartId,
                UserId = cart.UserId ?? Guid.Empty,
                CreatedAt = cart.CreatedAt ?? DateTime.UtcNow,
                Status = cart.Status ?? CartStatusEnum.ACTIVE.ToString(),
                CartItems = cartItems,
                TotalAmount = totalAmount
            };
        }

        /// <summary>
        /// Tạo BaseResponse với status code, message và data
        /// </summary>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="message">Thông báo kết quả</param>
        /// <param name="data">Dữ liệu trả về (optional)</param>
        /// <returns>BaseResponse để trả về controller</returns>
        private BaseResponse CreateResponse(int statusCode, string message, object? data = null)
        {
            return new BaseResponse
            {
                Status = statusCode.ToString(),
                Message = message,
                Data = data
            };
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Class helper để trả về kết quả validation
        /// </summary>
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        #endregion
    }
}