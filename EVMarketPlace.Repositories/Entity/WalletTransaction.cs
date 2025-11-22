using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class WalletTransaction
{
    public Guid WalletTransactionId { get; set; }

    public Guid WalletId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal? Amount { get; set; }

    public decimal? BalanceBefore { get; set; }

    public decimal? BalanceAfter { get; set; }

    public string? ReferenceId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Wallet Wallet { get; set; } = null!;
}
