using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class TourBooking
{
    public int TourBookingId { get; set; }

    public int TourGroupId { get; set; }

    public int UserId { get; set; }

    public string? Status { get; set; }

    public DateTime BookingDate { get; set; }

    public virtual TourGroup TourGroup { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
