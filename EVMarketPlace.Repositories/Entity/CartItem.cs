using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class CartItem
{
    public Guid CartItemId { get; set; }

    public Guid CartId { get; set; }

    public Guid PostId { get; set; }

    public int Quantity { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual ShoppingCart Cart { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
