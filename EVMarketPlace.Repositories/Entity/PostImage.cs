using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class PostImage
{
    public Guid ImageId { get; set; }

    public Guid PostId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
