using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class PriceHistory
{
    public int PriceHistoryId { get; set; }

    public int ProductId { get; set; }

    public decimal OldPrice { get; set; }

    public decimal NewPrice { get; set; }

    public DateTime ChangeAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
