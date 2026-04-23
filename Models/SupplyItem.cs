using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class SupplyItem
{
    public int SupplyItemId { get; set; }

    public int SupplyId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Supply Supply { get; set; } = null!;
}
