using System.ComponentModel.DataAnnotations;

namespace EVMarketPlace.Repositories.RequestDTO.Posts
{
    // DTO tạo mới Post

    public class PostCreateRequest
    {

        [Required, MinLength(3)]
        public string Type { get; set; } = null!;   // vehicle | battery

        [Required, MinLength(3)]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }


    }
}
