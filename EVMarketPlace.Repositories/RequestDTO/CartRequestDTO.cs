using System.ComponentModel.DataAnnotations;

namespace EVMarketPlace.Repositories.RequestDTO
{
    public class AddToCartRequest
    {
        [Required(ErrorMessage = "PostId is required")]
        public Guid PostId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        [Required(ErrorMessage = "CartItemId is required")]
        public Guid CartItemId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    public class RemoveFromCartRequest
    {
        [Required(ErrorMessage = "CartItemId is required")]
        public Guid CartItemId { get; set; }
    }
}