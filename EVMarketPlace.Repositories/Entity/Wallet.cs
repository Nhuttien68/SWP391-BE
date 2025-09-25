using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Wallet
{
    public Guid WalletId { get; set; }

    public Guid UserId { get; set; }

    public decimal Balance { get; set; }

    public DateTime LastUpdated { get; set; }

    public virtual User User { get; set; } = null!;
}
