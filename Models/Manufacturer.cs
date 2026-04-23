using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class Manufacturer
{
    public int ManufactureId { get; set; }

    public string Name { get; set; } = null!;

    public string? Country { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
