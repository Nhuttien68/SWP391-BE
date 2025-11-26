using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Transaction
{
    public Guid TransactionId { get; set; }

    public Guid? BuyerId { get; set; }

    public Guid? SellerId { get; set; }

    public Guid? PostId { get; set; }

    public Guid? CartId { get; set; }

    public decimal? Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ReceiverName { get; set; }

    public string? ReceiverPhone { get; set; }

    public string? ReceiverAddress { get; set; }

    public string? Note { get; set; }

    public decimal? CommissionRate { get; set; }

    public decimal? CommissionAmount { get; set; }

    public decimal? SellerReceived { get; set; }

    public virtual User? Buyer { get; set; }

    public virtual ShoppingCart? Cart { get; set; }

    public virtual Post? Post { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User? Seller { get; set; }
}
