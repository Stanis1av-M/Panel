using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class FavoriteProduct
{
    public int FavoriteProductId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public DateTime AddedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
