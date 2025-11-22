using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class WithdrawalRequest
{
    public Guid WithdrawalId { get; set; }

    public Guid UserId { get; set; }

    public Guid? WalletId { get; set; }

    public decimal? Amount { get; set; }

    public string? BankName { get; set; }

    public string? BankAccountNumber { get; set; }

    public string? BankAccountName { get; set; }

    public string? Status { get; set; }

    public DateTime? RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public Guid? ProcessedBy { get; set; }

    public string? Note { get; set; }

    public string? AdminNote { get; set; }

    public virtual User? ProcessedByNavigation { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Wallet? Wallet { get; set; }
}
