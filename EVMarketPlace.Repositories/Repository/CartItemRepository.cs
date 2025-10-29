using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace EVMarketPlace.Repositories.Repository
{
    public class CartItemRepository : GenericRepository<CartItem>
    {
        public async Task<CartItem?> GetCartItemByIdAsync(Guid cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Post)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<CartItem?> GetCartItemByPostAsync(Guid cartId, Guid postId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.PostId == postId);
        }

        public async Task<List<CartItem>> GetCartItemsByCartIdAsync(Guid cartId)
        {
            return await _context.CartItems
                .Include(ci => ci.Post)
                .ThenInclude(p => p.PostImages)
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();
        }

        public async Task<CartItem> CreateCartItemAsync(CartItem cartItem)
        {
            await CreateAsync(cartItem);
            return cartItem;
        }

        public async Task<CartItem> UpdateCartItemAsync(CartItem cartItem)
        {
            await UpdateAsync(cartItem);
            return cartItem;
        }

        public async Task DeleteCartItemAsync(Guid cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem != null)
            {
                await RemoveAsync(cartItem);
            }
        }
    }
}