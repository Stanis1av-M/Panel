using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class DeliveryMethod
{
    public int DeliveryMethodId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
