namespace EVMarketPlace.Repositories.ResponseDTO;

public class PostListItemDto
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
