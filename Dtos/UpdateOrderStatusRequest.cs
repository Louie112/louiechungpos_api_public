using System.ComponentModel.DataAnnotations;
using louiechungpos.Models;

namespace louiechungpos.Dtos;

public class UpdateOrderStatusRequest
{
    [Required]
    public OrderStatus Status { get; set; }
}