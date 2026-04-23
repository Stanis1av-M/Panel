using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int ManufacturerId { get; set; }

    public int SupplierId { get; set; }

    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string Article { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public decimal Price { get; set; }

    public decimal? OldPrice { get; set; }

    public int Discount { get; set; }

    public int Stock { get; set; }

    public string? AvailabilityStatus { get; set; }

    public string? Description { get; set; }

    public bool IsVisible { get; set; }

    public string? Tags { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual ICollection<SupplyItem> SupplyItems { get; set; } = new List<SupplyItem>();
}
