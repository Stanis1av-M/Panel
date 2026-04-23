using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public int OrderStatusId { get; set; }

    public int DeliveryMethodId { get; set; }

    public int PaymentMethodId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual DeliveryMethod DeliveryMethod { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual OrderStatus OrderStatus { get; set; } = null!;

    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
