using System.ComponentModel.DataAnnotations;
using louiechungpos.Models;

namespace louiechungpos.Dtos;

public class CreateRestaurantRequest
{
    [Required] public string Name { get; set; } = default!;
    public string? Address { get; set; }
    [Required] public int UserId { get; set; }
}

public record UserRoleDto(int UserId, string UserName, string Role);

public record RestaurantAdminResponse(int Id, string Name, string? Address, RestaurantApprovalStatus Status, IEnumerable<UserRoleDto> UserRoles);
public record RestaurantCreateResponse(int Id, string Name, string? Address, RestaurantApprovalStatus Status);
public record RestaurantSimpleResponse(int Id, string Name, string? Address);

