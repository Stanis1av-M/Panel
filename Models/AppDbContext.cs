using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace panel.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ComplexityLevel> ComplexityLevels { get; set; }

    public virtual DbSet<DeliveryMethod> DeliveryMethods { get; set; }

    public virtual DbSet<FavoriteProduct> FavoriteProducts { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<News> News { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PriceHistory> PriceHistories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Supply> Supplies { get; set; }

    public virtual DbSet<SupplyItem> SupplyItems { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<TourBooking> TourBookings { get; set; }

    public virtual DbSet<TourGroup> TourGroups { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=ApexDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(e => e.ProductId, "IX_CartItems_ProductId");

            entity.HasIndex(e => e.UserId, "IX_CartItems_UserId");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems).HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.User).WithMany(p => p.CartItems).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<DeliveryMethod>(entity =>
        {
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<FavoriteProduct>(entity =>
        {
            entity.HasIndex(e => e.ProductId, "IX_FavoriteProducts_ProductId");

            entity.HasIndex(e => e.UserId, "IX_FavoriteProducts_UserId");

            entity.HasOne(d => d.Product).WithMany(p => p.FavoriteProducts).HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.User).WithMany(p => p.FavoriteProducts).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Feedbacks_UserId");

            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.AdminResponse).HasMaxLength(300);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.ManufactureId);
        });

        modelBuilder.Entity<News>(entity =>
        {
            entity.Property(e => e.NewsId).HasColumnName("NewsID");
            entity.Property(e => e.Content).HasMaxLength(2500);
            entity.Property(e => e.ImageUrl).HasColumnName("ImageURL");
            entity.Property(e => e.ImageUrl2).HasColumnName("ImageURL2");
            entity.Property(e => e.Title).HasMaxLength(80);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.DeliveryMethodId, "IX_Orders_DeliveryMethodId");

            entity.HasIndex(e => e.OrderStatusId, "IX_Orders_OrderStatusId");

            entity.HasIndex(e => e.PaymentMethodId, "IX_Orders_PaymentMethodId");

            entity.HasIndex(e => e.UserId, "IX_Orders_UserId");

            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.DeliveryMethod).WithMany(p => p.Orders)
                .HasForeignKey(d => d.DeliveryMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OrderStatus).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_OrderItems_OrderId");

            entity.HasIndex(e => e.ProductId, "IX_OrderItems_ProductId");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasForeignKey(d => d.OrderId);

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.Property(e => e.OrderStatusId).HasColumnName("OrderStatusID");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasIndex(e => e.ProductId, "IX_PriceHistories_ProductId");

            entity.Property(e => e.NewPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OldPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.PriceHistories).HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_Products_CategoryId");

            entity.HasIndex(e => e.ManufacturerId, "IX_Products_ManufacturerId");

            entity.HasIndex(e => e.SupplierId, "IX_Products_SupplierId");

            entity.Property(e => e.Article).HasMaxLength(30);
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.OldPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Products).HasForeignKey(d => d.ManufacturerId);

            entity.HasOne(d => d.Supplier).WithMany(p => p.Products).HasForeignKey(d => d.SupplierId);
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasIndex(e => e.ProductId, "IX_ProductReviews_ProductId");

            entity.HasIndex(e => e.UserId, "IX_ProductReviews_UserId");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews).HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.User).WithMany(p => p.ProductReviews).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Supply>(entity =>
        {
            entity.HasIndex(e => e.SupplierId, "IX_Supplies_SupplierId");

            entity.HasIndex(e => e.UserId, "IX_Supplies_UserId");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Supplies).HasForeignKey(d => d.SupplierId);

            entity.HasOne(d => d.User).WithMany(p => p.Supplies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SupplyItem>(entity =>
        {
            entity.HasIndex(e => e.ProductId, "IX_SupplyItems_ProductId");

            entity.HasIndex(e => e.SupplyId, "IX_SupplyItems_SupplyId");

            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.SupplyItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Supply).WithMany(p => p.SupplyItems).HasForeignKey(d => d.SupplyId);
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasIndex(e => e.ComplexityLevelId, "IX_Tours_ComplexityLevelId");

            entity.HasOne(d => d.ComplexityLevel).WithMany(p => p.Tours).HasForeignKey(d => d.ComplexityLevelId);
        });

        modelBuilder.Entity<TourBooking>(entity =>
        {
            entity.HasIndex(e => e.TourGroupId, "IX_TourBookings_TourGroupId");

            entity.HasIndex(e => e.UserId, "IX_TourBookings_UserId");

            entity.HasOne(d => d.TourGroup).WithMany(p => p.TourBookings).HasForeignKey(d => d.TourGroupId);

            entity.HasOne(d => d.User).WithMany(p => p.TourBookings)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TourGroup>(entity =>
        {
            entity.HasIndex(e => e.GuideId, "IX_TourGroups_GuideId");

            entity.HasIndex(e => e.TourId, "IX_TourGroups_TourId");

            entity.HasOne(d => d.Guide).WithMany(p => p.TourGroups)
                .HasForeignKey(d => d.GuideId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tour).WithMany(p => p.TourGroups).HasForeignKey(d => d.TourId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_Users_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasForeignKey(d => d.RoleId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
