using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class ProductReview
{
    public int ProductReviewId { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public string ReviewText { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsApproved { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
