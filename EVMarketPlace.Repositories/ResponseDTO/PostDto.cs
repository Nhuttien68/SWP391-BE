using EVMarketPlace.Repositories.RequestDTO;

namespace EVMarketPlace.Repositories.ResponseDTO.Posts
{
    //DTO trả về client (giấu navigation, chỉ field cần).
    public class PostDto
    {
      
    }
    public class PostResponseDto
    {
        public Guid PostId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; } = null!;
        public decimal? Price { get; set; }
        public string Type { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
         public DateTime ExpireAt { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<Guid>? ImgId { get; set; }
        public VehicleDto? Vehicle { get; set; }
        public BatteryDto? Battery { get; set; }
        public string Status { get; set; } 
         public PostPackgeDTOResponse PostDetail { get; set; }
        public UserinformationResponse User { get; set; }

    }
    public class VehicleDto
    {
        public Guid VehicleId { get; set; }
        public string BrandName { get; set; } = null!;
        public string Model { get; set; } = null!;
        public int? Year { get; set; }
        public int? Mileage { get; set; }
    }
    public class BatteryDto
    {
        public Guid BatteryId { get; set; }
        public string BrandName { get; set; } = null!;
        public int? Capacity { get; set; }
        public string Condition { get; set; } = null!;
    }
}
