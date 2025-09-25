using louiechungpos.Data;
using louiechungpos.Dtos;
using louiechungpos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace louiechungpos.Controllers;

[ApiController]
[Route("restaurants")]
public class RestaurantsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public RestaurantsController(ApplicationDbContext db) => _db = db;

    // GET /restaurants/{id}/menu
    [HttpGet("{id:int}/menu")]
    public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenu(int id)
    {
        var exists = await _db.Restaurants.AnyAsync(r => r.Id == id);
        if (!exists) return NotFound(new { message = $"Restaurant {id} not found." });

        var items = await _db.MenuItems
            .Where(m => m.RestaurantId == id && m.IsAvailable)
            .Select(m => new MenuItemDto(m.Id, m.Name, m.Description, m.Price, m.IsAvailable))
            .ToListAsync();

        return Ok(items);
    }

    // POST /restaurants
    [HttpPost]
    public async Task<ActionResult<RestaurantCreateResponse>> CreateRestaurant(CreateRestaurantRequest request)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
            return BadRequest(new { message = $"User {request.UserId} not found." });

        var restaurant = new Restaurant
        {
            Name = request.Name,
            Address = request.Address,
            Status = RestaurantApprovalStatus.Pending,
            UserRoles = new List<UserRestaurantRole>
            {
                new UserRestaurantRole
                {
                    UserId = request.UserId,
                    Role = UserRestaurantRoleStatus.Owner
                }
            }
        };

        _db.Restaurants.Add(restaurant);
        await _db.SaveChangesAsync();

        var response = new RestaurantCreateResponse(
            restaurant.Id,
            restaurant.Name,
            restaurant.Address,
            restaurant.Status
        );

        return CreatedAtAction(nameof(GetRestaurant), new { id = restaurant.Id }, response);
    }

    // GET /restaurants
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RestaurantSimpleResponse>>> BrowseRestaurants(
        [FromQuery] string? name,
        [FromQuery] RestaurantApprovalStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Restaurants.AsQueryable();

        // Filter
        if (!string.IsNullOrWhiteSpace(name))
        {
            var terms = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var term in terms)
            {
                var t = term.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(t));
            }
        }

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        // Only show active restaurants
        query = query.Where(r => r.Status == RestaurantApprovalStatus.Active);

        // Paging
        var restaurants = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RestaurantSimpleResponse(
                r.Id,
                r.Name,
                r.Address
            ))
            .ToListAsync();

        return Ok(restaurants);
    }

    // GET /restaurants/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RestaurantSimpleResponse>> GetRestaurant(int id)
    {
        var restaurant = await _db.Restaurants
            .Where(r => r.Id == id)
            .Select(r => new RestaurantSimpleResponse(
                r.Id,
                r.Name,
                r.Address
            ))
            .FirstOrDefaultAsync();

        if (restaurant is null)
            return NotFound(new { message = $"Restaurant {id} not found." });

        return Ok(restaurant);
    }

    // GET /restaurants/{id}/detailed
    [HttpGet("{id:int}/detailed")]
    public async Task<ActionResult<RestaurantAdminResponse>> GetRestaurantDetails(int id)
    {
        var restaurant = await _db.Restaurants
            .Include(r => r.UserRoles)
                .ThenInclude(ur => ur.User)
            .Where(r => r.Id == id)
            .Select(r => new RestaurantAdminResponse(
                r.Id,
                r.Name,
                r.Address,
                r.Status,
                r.UserRoles.Select(ur =>
                    new UserRoleDto(
                        ur.UserId,
                        ur.User.Name,
                        ur.Role.ToString()
                    )
                )
            ))
            .FirstOrDefaultAsync();

        if (restaurant is null)
            return NotFound(new { message = $"Restaurant {id} not found." });

        return Ok(restaurant);
    }

    // PATCH /restaurants/{id}
    [HttpPatch("{id:int}")]
    public async Task<ActionResult<RestaurantSimpleResponse>> UpdateRestaurantDetails(int id, [FromBody] UpdateRestaurantDetailsRequest req)
    {
        var restaurant = await _db.Restaurants.FindAsync(id);
        if (restaurant is null)
            return NotFound(new { message = $"Restaurant {id} not found." });

        restaurant.Name = req.Name;
        restaurant.Address = req.Address;

        await _db.SaveChangesAsync();

        var response = new RestaurantSimpleResponse(
            restaurant.Id,
            restaurant.Name,
            restaurant.Address
        );

        return Ok(response);
    }
    
    // PATCH /restaurants/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult> UpdateRestaurantStatus(int id, [FromBody] UpdateRestaurantStatusRequest req)
    {
        var restaurant = await _db.Restaurants.FindAsync(id);
        if (restaurant is null)
            return NotFound(new { message = $"Restaurant {id} not found." });

        var current = restaurant.Status;
        var newStatus = req.Status;

        // Transition rules
        bool allowed = current switch
        {
            RestaurantApprovalStatus.Pending => newStatus is RestaurantApprovalStatus.Active or RestaurantApprovalStatus.Rejected,
            RestaurantApprovalStatus.Active => newStatus is RestaurantApprovalStatus.Suspended,
            RestaurantApprovalStatus.Rejected => newStatus == RestaurantApprovalStatus.Rejected,
            RestaurantApprovalStatus.Suspended => newStatus is RestaurantApprovalStatus.Active, 
            _ => false
        };
        if (!allowed) return BadRequest(new { message = $"Cannot change status from {current} to {newStatus}." });

        restaurant.Status = newStatus;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
