using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Favorite
{
    public Guid FavoriteId { get; set; }

    public Guid UserId { get; set; }

    public Guid PostId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
