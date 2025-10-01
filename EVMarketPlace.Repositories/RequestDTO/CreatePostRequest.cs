using System.ComponentModel.DataAnnotations;

namespace EVMarketPlace.Repositories.RequestDTO
{
    public class CreatePostRequest
    {
        [Required]
        public required Guid UserId { get; set; }
        
        [Required, MaxLength(10)]
        public required string Type { get; set; }
        
        [Required, MaxLength(200)]
        public required string Title { get; set; }
        
        public string? Description { get; set; }
        
        [Range(1, double.MaxValue)]
        public decimal Price { get; set; }
    }
}
