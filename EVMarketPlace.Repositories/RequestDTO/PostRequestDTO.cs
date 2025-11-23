using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EVMarketPlace.Repositories.RequestDTO.Posts
{
    // DTO tạo mới Post

    public class PostRequestDTO
    { }

    public class PostCreateVehicleRequest
    {
        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }
        [Required(ErrorMessage ="Price not null"), Range(1, double.MaxValue, ErrorMessage = "Price must be a non-negative value")]
        public decimal? Price { get; set; }
        [Required]
        public Guid postPackgeID { get; set; }
        public VehicleCreateDto vehicleCreateDto { get; set; }

        public List<IFormFile> Images { get; set; }

    }
    public class VehicleCreateDto
    {
        [Required(ErrorMessage = "BrandId is required")]
        public Guid BrandId { get; set; }
        [Required(ErrorMessage = "Model is required")]
        public string Model { get; set; }
        [Required(ErrorMessage = "Year is required")]
        public int Year { get; set; }
        [Required(ErrorMessage = "Mileage is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Mileage must be a non-negative value")]
        public int Mileage { get; set; }
    }

    public class PostCreateBatteryRequest
    {
        [Required(ErrorMessage = "Title is required")]  
        public string? Title { get; set; }
        [Required(ErrorMessage = "Description is required")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "Price not null"), Range(1, double.MaxValue, ErrorMessage = "Price must be a non-negative value")]
        public decimal? Price { get; set; }
        [Required]
        public Guid postPackgeID { get; set; }
        public BatteryCreateDto batteryCreateDto { get; set; }
        public List<IFormFile> Images { get; set; }
    }
    public class BatteryCreateDto
    {
        [Required(ErrorMessage = "BranId is required")]
        public Guid BranId { get; set; }
        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a non-negative value")]
        public int Capacity { get; set; } 
        [Required(ErrorMessage = "Condition is required")]
        public string Condition { get; set; } 
    }
}
