using Microsoft.AspNetCore.Http;

public class PostUpdateRequest
{
}

public class UpdateBatteryPostRequest
{
    public Guid PostId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public Guid? BrandId { get; set; }
    public int? Capacity { get; set; }
    public string? Condition { get; set; }
    public List<Guid>? KeepImageIds { get; set; }
    public List<IFormFile>? NewImages { get; set; }
}
public class UpdateVehiclePostRequest
{
    public Guid PostId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public Guid? BrandId { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public int? Mileage { get; set; }
    public List<Guid>? KeepImageIds { get; set; }
    public List<IFormFile>? NewImages { get; set; }
}