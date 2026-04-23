using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class OrderStatus
{
    public int OrderStatusId { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
