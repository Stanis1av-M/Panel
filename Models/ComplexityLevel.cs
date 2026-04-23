using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class ComplexityLevel
{
    public int ComplexityLevelId { get; set; }

    public string Name { get; set; } = null!;

    public string? ColorCode { get; set; }

    public virtual ICollection<Tour> Tours { get; set; } = new List<Tour>();
}
