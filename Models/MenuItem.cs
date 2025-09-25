using System.ComponentModel.DataAnnotations;

namespace louiechungpos.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required]
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = default!;

    [Required, MaxLength(160)]
    public string Name { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, 100000)]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;
}
