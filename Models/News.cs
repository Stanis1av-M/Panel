using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class News
{
    public int NewsId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsArchived { get; set; }

    public string? ImageUrl2 { get; set; }

    public bool IsDeleted { get; set; }
}
