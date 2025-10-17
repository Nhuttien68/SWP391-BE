using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class PostUpdateRequest
{
}

public class UpdateBatteryPostRequest
{
    [Required(ErrorMessage ="PostId not allow null")]
    public Guid PostId { get; set; }
    [Required(ErrorMessage = "Title is required")]
    public string? Title { get; set; }
    [Required(ErrorMessage = "Description is required")]
    public string? Description { get; set; }
    [Required(ErrorMessage ="Price is not allow null")]
    [Range(1, double.MaxValue, ErrorMessage = "Price must be a non-negative value")]
    public decimal? Price { get; set; }
    [Required(ErrorMessage ="BrandId is required")]
    public Guid? BrandId { get; set; }
    [Required(ErrorMessage ="Model is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be a non-negative value")]
    public int? Capacity { get; set; }
    [Required(ErrorMessage ="Condition is required")]
    public string? Condition { get; set; }
    public List<Guid>? KeepImageIds { get; set; }
    public List<IFormFile>? NewImages { get; set; }
}
public class UpdateVehiclePostRequest
{
    [ Required(ErrorMessage = "PostId not allow null")]
    public Guid PostId { get; set; }
    [Required(ErrorMessage = "Title is required")]
    public string? Title { get; set; }
    [Required(ErrorMessage = "Description is required")]
    public string? Description { get; set; }
    [Required(ErrorMessage = "Price is not allow null")]
    public decimal? Price { get; set; }
    [Required(ErrorMessage = "BrandId is required")]
    public Guid? BrandId { get; set; }
    [Required(ErrorMessage = "Model is required")]   
    public string? Model { get; set; }
    [Required(ErrorMessage = "Year is required")]
    public int? Year { get; set; }
    [Required(ErrorMessage = "Mileage is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Mileage must be a non-negative value")]
    public int? Mileage { get; set; }
    public List<Guid>? KeepImageIds { get; set; }
    public List<IFormFile>? NewImages { get; set; }
}