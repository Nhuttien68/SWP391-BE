using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Vehicle
{
    public Guid VehicleId { get; set; }

    public Guid? PostId { get; set; }

    public Guid? BrandId { get; set; }

    public string? Model { get; set; }

    public int? Year { get; set; }

    public int? Mileage { get; set; }

    public virtual VehicleBrand? Brand { get; set; }

    public virtual Post? Post { get; set; }
}
