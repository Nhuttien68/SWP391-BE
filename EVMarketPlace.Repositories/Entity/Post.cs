using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Post
{
    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public Guid PackageId { get; set; }

    public decimal PackagePrice { get; set; }

    public string? Type { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime ExpireAt { get; set; }

    public string? Status { get; set; }

    public virtual Auction? Auction { get; set; }

    public virtual Battery? Battery { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual PostPackage Package { get; set; } = null!;

    public virtual ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual User User { get; set; } = null!;

    public virtual Vehicle? Vehicle { get; set; }
}
