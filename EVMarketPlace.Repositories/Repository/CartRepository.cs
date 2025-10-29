using EVMarketPlace.Repositories.Entity;
using Microsoft.EntityFrameworkCore;

namespace EVMarketPlace.Repositories.Repository
{
    public class CartRepository : GenericRepository<ShoppingCart>
    {
        public async Task<ShoppingCart?> GetCartByUserIdAsync(Guid userId)
        {
            return await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Post)
                .ThenInclude(p => p.PostImages)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<ShoppingCart?> GetCartByIdAsync(Guid cartId)
        {
            return await _context.ShoppingCarts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Post)
                .ThenInclude(p => p.PostImages)
                .FirstOrDefaultAsync(c => c.CartId == cartId);
        }

        public async Task<ShoppingCart> CreateCartAsync(ShoppingCart cart)
        {
            await CreateAsync(cart);
            return cart;
        }

        public async Task<ShoppingCart> UpdateCartAsync(ShoppingCart cart)
        {
            await UpdateAsync(cart);
            return cart;
        }

        public async Task DeleteCartAsync(Guid cartId)
        {
            var cart = await _context.ShoppingCarts.FindAsync(cartId);
            if (cart != null)
            {
                await RemoveAsync(cart);
            }
        }
    }
}