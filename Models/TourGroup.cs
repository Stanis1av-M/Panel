using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class TourGroup
{
    public int TourGroupId { get; set; }

    public int TourId { get; set; }

    public int GuideId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int MaxCapacity { get; set; }

    public virtual User Guide { get; set; } = null!;

    public virtual Tour Tour { get; set; } = null!;

    public virtual ICollection<TourBooking> TourBookings { get; set; } = new List<TourBooking>();
}
