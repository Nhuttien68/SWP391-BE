using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class VehicleBrand
{
    public Guid BrandId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
