using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public string? AdminResponse { get; set; }

    public int Status { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
