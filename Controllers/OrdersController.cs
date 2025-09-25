using louiechungpos.Data;
using louiechungpos.Dtos;
using louiechungpos.Hubs;
using louiechungpos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace louiechungpos.Controllers;

[ApiController]
[Route("orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<OrdersHub> _hubContext;
    public OrdersController(ApplicationDbContext db, IHubContext<OrdersHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    // POST /orders
    [Authorize(Policy = "OrdersReadWritePolicy")]
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest req)
    {
        // Validate related entities exist
        var user = await _db.Users.FindAsync(req.UserId);
        if (user is null) return BadRequest(new { message = $"User {req.UserId} does not exist." });

        var restaurant = await _db.Restaurants.FindAsync(req.RestaurantId);
        if (restaurant is null) return BadRequest(new { message = $"Restaurant {req.RestaurantId} does not exist." });

        if (req.Items is null || req.Items.Count == 0)
            return BadRequest(new { message = "Items are required." });

        // Load all menu items referenced and ensure they belong to the same restaurant
        var menuItemIds = req.Items.Select(i => i.MenuItemId).Distinct().ToList();

        var menuItems = await _db.MenuItems
            .Where(mi => mi.RestaurantId == req.RestaurantId && menuItemIds.Contains(mi.Id) && mi.IsAvailable)
            .ToListAsync();

        if (menuItems.Count != menuItemIds.Count)
            return BadRequest(new { message = "One or more menu items are invalid or not available for this restaurant." });

        // Build order + items
        var order = new Order
        {
            UserId = req.UserId,
            RestaurantId = req.RestaurantId,
            Status = OrderStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in req.Items)
        {
            var menuItem = menuItems.First(mi => mi.Id == item.MenuItemId);
            order.Items.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                Quantity = item.Quantity,
                UnitPrice = menuItem.Price
            });
        }

        order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var response = new OrderResponse(
            order.Id,
            order.Status.ToString(),
            order.Total,
            order.CreatedAtUtc,
            order.Items.Select(oi => new OrderLineDto(
                oi.MenuItemId,
                menuItems.First(mi => mi.Id == oi.MenuItemId).Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.UnitPrice * oi.Quantity
            )).ToList()
        );

        await _hubContext.Clients.All.SendAsync("OrderCreated", response);

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, response);
    }

    // Helper for CreatedAtAction
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        var resp = new OrderResponse(
            order.Id,
            order.Status.ToString(),
            order.Total,
            order.CreatedAtUtc,
            order.Items.Select(oi => new OrderLineDto(
                oi.MenuItemId,
                oi.MenuItem.Name,
                oi.Quantity,
                oi.UnitPrice,
                oi.UnitPrice * oi.Quantity
            )).ToList()
        );

        return Ok(resp);
    }

    // PATCH /orders/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest req)
    {
        var newStatus = req.Status;

        var order = await _db.Orders.FindAsync(id);
        if (order is null) return NotFound(new { message = $"Order {id} not found." });

        // Transition rules
        bool allowed = order.Status switch
        {
            OrderStatus.Pending => newStatus is OrderStatus.Confirmed or OrderStatus.Cancelled,
            OrderStatus.Confirmed => newStatus is OrderStatus.Preparing or OrderStatus.Cancelled,
            OrderStatus.Preparing => newStatus is OrderStatus.Ready or OrderStatus.Cancelled,
            OrderStatus.Ready => newStatus is OrderStatus.Completed or OrderStatus.Cancelled,
            OrderStatus.Completed => newStatus == OrderStatus.Completed, // no-op
            OrderStatus.Cancelled => newStatus == OrderStatus.Cancelled, // no-op
            _ => false
        };
        if (!allowed) return BadRequest(new { message = $"Cannot change status from {order.Status} to {newStatus}." });

        order.Status = newStatus;
        await _db.SaveChangesAsync();

        await _hubContext.Clients.All.SendAsync("OrderUpdated", new
        {
            order.Id,
            Status = order.Status.ToString()
        });

        return NoContent();
    }
}
