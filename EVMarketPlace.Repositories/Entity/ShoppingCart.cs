using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class ShoppingCart
{
    public Guid CartId { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual User User { get; set; } = null!;
}
