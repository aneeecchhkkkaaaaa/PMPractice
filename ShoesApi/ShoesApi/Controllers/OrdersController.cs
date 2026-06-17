using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoesApi.Data;
using ShoesApi.Models;

namespace ShoesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly is25_practice_shoesContext _context;

        public OrdersController(is25_practice_shoesContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.Status)
                .OrderByDescending(o => o.OrderId)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.Status)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound();
            return order;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(OrderCreateDto dto)
        {
            var random = new Random();
            int code = random.Next(100000, 999999);
            var order = new Order
            {
                OrderDate = dto.OrderDate,
                DeliveryDate = dto.DeliveryDate,
                AddressId = dto.AddressId,
                UserId = dto.UserId,
                СodeForReceipt = code,
                StatusId = dto.StatusId
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await _context.Entry(order).Reference(o => o.Address).LoadAsync();
            await _context.Entry(order).Reference(o => o.Status).LoadAsync();
            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, OrderUpdateDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            order.OrderDate = dto.OrderDate;
            order.DeliveryDate = dto.DeliveryDate;
            order.AddressId = dto.AddressId;
            order.StatusId = dto.StatusId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrdersProducts)
                .FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null) return NotFound();
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class OrderCreateDto
    {
        public DateOnly OrderDate { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public int AddressId { get; set; }
        public int StatusId { get; set; }
        public int UserId { get; set; }
    }

    public class OrderUpdateDto
    {
        public DateOnly OrderDate { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public int AddressId { get; set; }
        public int StatusId { get; set; }
    }
}