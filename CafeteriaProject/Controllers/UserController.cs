using CafeteriaProject.Models;
using CafeteriaProject.Models.Data;
using CafeteriaProject.Models.DTO_s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CafeteriaProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: /api/orders
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid token.");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.UserId == int.Parse(userId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var orderDtos = orders.Select(o => new
            {
                o.Id,
                o.OrderDate,
                o.TotalPrice,
                OrderItems = o.OrderItems.Select(oi => new
                {
                    oi.MenuItemId,
                    oi.MenuItem.ItemName,
                    oi.Price,
                    oi.Quantity
                })
            });

            return Ok(new { orders = orderDtos, isSuccess = true });
        }

        // ✅ GET: /api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid token.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == int.Parse(userId));

            if (order == null)
                return NotFound(new { message = "Order not found", isSuccess = false });

            var orderDto = new
            {
                order.Id,
                order.OrderDate,
                order.TotalPrice,
                OrderItems = order.OrderItems.Select(oi => new
                {
                    oi.MenuItemId,
                    oi.MenuItem.ItemName,
                    oi.Price,
                    oi.Quantity
                })
            };

            return Ok(new { order = orderDto, isSuccess = true });
        }

        // ✅ POST: /api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid token.");

            var order = new Order
            {
                UserId = int.Parse(userId),
                OrderDate = DateTime.UtcNow
            };

            foreach (var item in dto.OrderItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                if (menuItem == null) continue;

                order.OrderItems.Add(new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = item.Quantity,
                    Price = menuItem.Price
                });
            }

            order.TotalPrice = order.OrderItems.Sum(i => i.Price * i.Quantity);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order created successfully", orderId = order.Id, isSuccess = true });
        }

        // ✅ PUT: /api/orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> EditOrder(int id, [FromBody] OrderDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid token.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == int.Parse(userId));

            if (order == null)
                return NotFound(new { message = "Order not found", isSuccess = false });

            // Clear old order items
            _context.OrderItems.RemoveRange(order.OrderItems);

            // Add new ones
            foreach (var item in dto.OrderItems)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
                if (menuItem == null) continue;

                order.OrderItems.Add(new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = item.Quantity,
                    Price = menuItem.Price
                });
            }

            order.TotalPrice = order.OrderItems.Sum(i => i.Price * i.Quantity);
            order.OrderDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Order updated successfully", isSuccess = true });
        }

        // ✅ GET: /api/orders/menu
        [AllowAnonymous]
        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu()
        {
            var items = await _context.MenuItems.ToListAsync();
            return Ok(new { menu = items, isSuccess = true });
        }
    }
}
