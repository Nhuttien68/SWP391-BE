using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class BatteryBrand
{
    public Guid BrandId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Battery> Batteries { get; set; } = new List<Battery>();
}
