using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class AuctionBid
{
    public Guid BidId { get; set; }

    public Guid? AuctionId { get; set; }

    public Guid? UserId { get; set; }

    public decimal? BidAmount { get; set; }

    public DateTime? BidTime { get; set; }

    public virtual Auction? Auction { get; set; }

    public virtual User? User { get; set; }
}
