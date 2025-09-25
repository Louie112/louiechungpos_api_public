using System.ComponentModel.DataAnnotations;

namespace louiechungpos.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = default!;

    [Required, MaxLength(200)]
    public string Email { get; set; } = default!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<UserRestaurantRole> RestaurantRoles { get; set; } = new List<UserRestaurantRole>();
}
