using System.ComponentModel.DataAnnotations;
using louiechungpos.Models;

namespace louiechungpos.Dtos;

public class UpdateRestaurantStatusRequest
{
    [Required]
    public RestaurantApprovalStatus Status { get; set; }
}
