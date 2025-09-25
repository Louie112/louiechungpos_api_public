using System.ComponentModel.DataAnnotations;

namespace louiechungpos.Models;

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;

    [Required]
    public int MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = default!;

    [Range(1, 9999)]
    public int Quantity { get; set; }

    [Range(0, 100000)]
    public decimal UnitPrice { get; set; }
}
