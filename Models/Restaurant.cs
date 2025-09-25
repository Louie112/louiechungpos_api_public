using System.ComponentModel.DataAnnotations;

namespace louiechungpos.Models;

public class Restaurant
{
    public int Id { get; set; }

    [Required, MaxLength(160)]
    public string Name { get; set; } = default!;

    [MaxLength(240)]
    public string? Address { get; set; }

    public RestaurantApprovalStatus Status { get; set; } = RestaurantApprovalStatus.Pending;

    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<UserRestaurantRole> UserRoles { get; set; } = new List<UserRestaurantRole>();
}
