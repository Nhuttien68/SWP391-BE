using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Transaction
{
    public Guid TransactionId { get; set; }

    public Guid BuyerId { get; set; }

    public Guid SellerId { get; set; }

    public Guid PostId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual Post Post { get; set; } = null!;

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User Seller { get; set; } = null!;
}
