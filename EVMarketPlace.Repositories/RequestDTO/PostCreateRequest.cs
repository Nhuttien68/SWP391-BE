using System.ComponentModel.DataAnnotations;

namespace EVMarketPlace.Repositories.RequestDTO.Posts
{
    // DTO tạo mới Post

    public class PostCreateRequest
    {
        [Required] public Guid UserId { get; set; }

        [Required, MinLength(3)]
        public string Type { get; set; } = null!;   // vehicle | battery

        [Required, MinLength(3)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        //Nếu null -> mặc định true (set trong service)
        public bool? IsActive { get; set; }
    }
}
