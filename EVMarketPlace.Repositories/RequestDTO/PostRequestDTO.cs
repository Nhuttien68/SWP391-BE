using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EVMarketPlace.Repositories.RequestDTO.Posts
{
    // DTO tạo mới Post

    public class PostRequestDTO
    { }

    public class PostCreateVehicleRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public VehicleCreateDto vehicleCreateDto { get; set; }

        public List<IFormFile> Images { get; set; }

    }
    public class VehicleCreateDto
    {
        public Guid BrandId { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Mileage { get; set; }
    }

    public class PostCreateBatteryRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public BatteryCreateDto batteryCreateDto { get; set; }
        public List<IFormFile> Images { get; set; }
    }
    public class BatteryCreateDto
    {
        public Guid BranId { get; set; }
        public int Capacity { get; set; } 
        public string Condition { get; set; } 
    }
}
