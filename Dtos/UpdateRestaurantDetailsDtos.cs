using System.ComponentModel.DataAnnotations;

namespace louiechungpos.Dtos;

public class UpdateRestaurantDetailsRequest
{
    [Required, MaxLength(160)] public string Name { get; set; } = default!;
    [MaxLength(240)] public string? Address { get; set; }
}
