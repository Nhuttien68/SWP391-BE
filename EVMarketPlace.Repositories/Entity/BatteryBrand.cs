
namespace EVMarketPlace.Repositories.Entity;

public partial class BatteryBrand
{
    public Guid BrandId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Battery> Batteries { get; set; } = new List<Battery>();
}
