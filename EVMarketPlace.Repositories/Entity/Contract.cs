using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Contract
{
    public Guid ContractId { get; set; }

    public Guid TransactionId { get; set; }

    public string? ContractFile { get; set; }

    public DateTime? SignedAt { get; set; }

    public virtual Transaction Transaction { get; set; } = null!;
}
