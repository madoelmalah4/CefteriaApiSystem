using CafeteriaProject.Models;
using CafeteriaProject.Models.Data;
using CafeteriaProject.Models.DTO_s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CafeteriaProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid or missing token.");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == int.Parse(userId))
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                OrderItems = o.OrderItems.Select(i => new OrderItemDto
                {
                    ItemName = i.ItemName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList();

            return Ok(new { orders = orderDtos, isSuccess = true });
        }

        // ✅ GET /api/user/orders/{id}
        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid or missing token.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == int.Parse(userId));

            if (order == null)
                return NotFound(new { message = "Order not found.", isSuccess = false });

            var orderDto = new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.Select(i => new OrderItemDto
                {
                    ItemName = i.ItemName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(new { order = orderDto, isSuccess = true });
        }

        // ✅ POST /api/user/orders
        [HttpPost("orders")]
        public async Task<IActionResult> AddOrder([FromBody] OrderDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid or missing token.");

            var order = new Order
            {
                UserId = int.Parse(userId),
                OrderDate = DateTime.UtcNow,
                OrderItems = dto.OrderItems.Select(i => new OrderItem
                {
                    ItemName = i.ItemName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            order.TotalPrice = order.OrderItems.Sum(i => i.Price * i.Quantity);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderDto = new OrderDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                OrderItems = order.OrderItems.Select(i => new OrderItemDto
                {
                    ItemName = i.ItemName,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, new { order = orderDto, isSuccess = true });
        }

        // ✅ PUT /api/user/orders/{id}
        [HttpPut("orders/{id}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid or missing token.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == int.Parse(userId));

            if (order == null)
                return NotFound(new { message = "Order not found.", isSuccess = false });

            // Remove old items
            _context.OrderItems.RemoveRange(order.OrderItems);

            // Add updated items
            order.OrderItems = dto.OrderItems.Select(i => new OrderItem
            {
                ItemName = i.ItemName,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList();

            // Update total
            order.TotalPrice = order.OrderItems.Sum(i => i.Price * i.Quantity);
            order.OrderDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Order updated successfully.", isSuccess = true });
        }

        // ✅ DELETE /api/user/orders/{id}
        [HttpDelete("orders/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("Invalid or missing token.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == int.Parse(userId));

            if (order == null)
                return NotFound(new { message = "Order not found.", isSuccess = false });

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Order deleted successfully.", isSuccess = true });
        }
    }
}
