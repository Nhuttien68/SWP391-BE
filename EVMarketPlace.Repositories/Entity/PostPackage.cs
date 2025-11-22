using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class PostPackage
{
    public Guid PackageId { get; set; }

    public string PackageName { get; set; } = null!;

    public int DurationInDays { get; set; }

    public decimal Price { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
