using System;
using System.Collections.Generic;

namespace panel.Models;

public partial class User
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string Email { get; set; } = null!;

    public DateTime RegistrationDate { get; set; }

    public string? Status { get; set; }

    public string Password { get; set; } = null!;

    public string? Description { get; set; }

    public bool Ban { get; set; }

    public string? PhotoPath { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<FavoriteProduct> FavoriteProducts { get; set; } = new List<FavoriteProduct>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Supply> Supplies { get; set; } = new List<Supply>();

    public virtual ICollection<TourBooking> TourBookings { get; set; } = new List<TourBooking>();

    public virtual ICollection<TourGroup> TourGroups { get; set; } = new List<TourGroup>();
}
