using System.ComponentModel.DataAnnotations;

namespace louiechungpos.Models;

public class UserRestaurantRole
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    [Required]
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = default!;

    public UserRestaurantRoleStatus Role { get; set; } = UserRestaurantRoleStatus.Owner; 
}
