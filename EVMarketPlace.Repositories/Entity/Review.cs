using System;
using System.Collections.Generic;

namespace EVMarketPlace.Repositories.Entity;

public partial class Review
{
    public Guid ReviewId { get; set; }

    public Guid TransactionId { get; set; }

    public Guid ReviewerId { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Reviewer { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
