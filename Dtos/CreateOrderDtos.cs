using System.ComponentModel.DataAnnotations;

namespace louiechungpos.Dtos;

public record CreateOrderItemDto(
    [Required] int MenuItemId,
    [Range(1, 9999)] int Quantity
);

public class CreateOrderRequest
{
    [Required] public int UserId { get; set; }
    [Required] public int RestaurantId { get; set; }
    [Required] public List<CreateOrderItemDto> Items { get; set; } = new();
}

public record OrderLineDto(int MenuItemId, string Name, int Quantity, decimal UnitPrice, decimal LineTotal);
public record OrderResponse(int Id, string Status, decimal Total, DateTime CreatedAtUtc, IEnumerable<OrderLineDto> Items);
