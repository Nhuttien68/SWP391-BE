using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Auction
{
    public Guid AuctionId { get; set; }

    public Guid? PostId { get; set; }

    public decimal? StartPrice { get; set; }

    public decimal? CurrentPrice { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<AuctionBid> AuctionBids { get; set; } = new List<AuctionBid>();

    public virtual Post? Post { get; set; }
}
