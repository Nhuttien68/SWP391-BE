namespace EVMarketPlace.Repositories.RequestDTO
{
    public class CreatePostRequest
    {
        public Guid UserId { get; set; }
        public string Type { get; set; } = "";     // "vehicle" | "battery"
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
    }
}
