namespace EVMarketPlace.Repositories.ResponseDTO.Posts
{
    //DTO trả về client (giấu navigation, chỉ field cần).
    public class PostDto
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
