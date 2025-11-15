// /Data/ApplicationDbContext.cs

using louiechungpos.Models;
using Microsoft.EntityFrameworkCore;

namespace louiechungpos.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<UserRestaurantRole> UserRestaurantRoles => Set<UserRestaurantRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Decimal precision for money fields
        modelBuilder.Entity<MenuItem>().Property(mi => mi.Price).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Order>().Property(o => o.Total).HasPrecision(18, 2);

        // OrderItem relationships
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.MenuItem)
            .WithMany()
            .HasForeignKey(oi => oi.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);

        //MenuItem relationships
        modelBuilder.Entity<MenuItem>()
            .HasOne(mi => mi.Restaurant)
            .WithMany(r => r.MenuItems)
            .HasForeignKey(mi => mi.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        //Order relationships
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Restaurant)
            .WithMany(r => r.Orders)
            .HasForeignKey(o => o.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);

        //UserRestaurantRole relationships
        modelBuilder.Entity<UserRestaurantRole>()
            .HasOne(urr => urr.User)
            .WithMany(u => u.RestaurantRoles)
            .HasForeignKey(urr => urr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRestaurantRole>()
            .HasOne(urr => urr.Restaurant)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(urr => urr.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // minimal seed data
        modelBuilder.Entity<User>().HasData(new User { Id = 1, Name = "Test User", Email = "test@example.com" });

        modelBuilder.Entity<Restaurant>().HasData(
            new Restaurant { Id = 1, Name = "Pizza Palace", Address = "123 Main St" }
        );

        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = 1, RestaurantId = 1, Name = "Margherita", Description = "Tomato, mozzarella, basil", Price = 12.50m, IsAvailable = true },
            new MenuItem { Id = 2, RestaurantId = 1, Name = "Pepperoni", Description = "Classic pepperoni", Price = 14.00m, IsAvailable = true },
            new MenuItem { Id = 3, RestaurantId = 1, Name = "Garlic Knots", Description = "6 pcs", Price = 6.00m, IsAvailable = true }
        );
    }
}