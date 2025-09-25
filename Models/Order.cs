using System.ComponentModel.DataAnnotations;

namespace louiechungpos.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    [Required]
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Range(0, 1000000)]
    public decimal Total { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public List<OrderItem> Items { get; set; } = new();
}

