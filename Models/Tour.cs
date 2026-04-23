using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class Tour
{
    public int TourId { get; set; }

    public int ComplexityLevelId { get; set; }

    public string Name { get; set; } = null!;

    public string? Region { get; set; }

    public string? Description { get; set; }

    public int DurationDays { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ComplexityLevel ComplexityLevel { get; set; } = null!;

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();
}
