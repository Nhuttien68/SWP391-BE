using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Battery
{
    public Guid BatteryId { get; set; }

    public Guid? PostId { get; set; }

    public Guid? BrandId { get; set; }

    public int? Capacity { get; set; }

    public string? Condition { get; set; }

    public virtual BatteryBrand? Brand { get; set; }

    public virtual Post? Post { get; set; }
}
