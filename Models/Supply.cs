using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class Supply
{
    public int SupplyId { get; set; }

    public int SupplierId { get; set; }

    public int UserId { get; set; }

    public string? Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompleteAt { get; set; }

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<SupplyItem> SupplyItems { get; set; } = new List<SupplyItem>();

    public virtual User User { get; set; } = null!;
}
