namespace EVMarketPlace.Repositories.ResponseDTO
{
    public class CartResponseDTO
    {
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = null!;
        public List<CartItemResponseDTO> CartItems { get; set; } = new List<CartItemResponseDTO>();
        public decimal TotalAmount { get; set; }
    }

    public class CartItemResponseDTO
    {
        public Guid CartItemId { get; set; }
        public Guid PostId { get; set; }
        public string PostTitle { get; set; } = null!;
        public string? PostDescription { get; set; }
        public decimal? Price { get; set; }
        public int Quantity { get; set; }
        public decimal? Subtotal { get; set; }
        public DateTime AddedAt { get; set; }
        public string? ImageUrl { get; set; }
    }
}