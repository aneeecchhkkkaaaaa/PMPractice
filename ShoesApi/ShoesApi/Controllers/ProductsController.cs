using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoesApi.Data;
using ShoesApi.Models;

namespace ShoesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly is25_practice_shoesContext _context;

        public ProductsController(is25_practice_shoesContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            [FromQuery] bool sortCount,
            [FromQuery] string? searchTerm = null)
        {
            var query = _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var words = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    var s = word.ToLower();
                    query = query.Where(p =>
                    (p.ProductName.ToLower().Contains(s)) ||
                    (p.Description.ToLower().Contains(s)) ||
                    (p.Category.ToLower().Contains(s)) ||
                    (p.Manufacturer.ManufacturerName.ToLower().Contains(s)) ||
                    (p.Supplier.SupplierName.ToLower().Contains(s)) ||
                    (p.UnitOfMeasurement.ToLower().Contains(s)));
                }
            }
            if (sortCount)
            {
                return await query.OrderBy(p => p.InWarehouse).ToListAsync();
            }
            else return await query.OrderByDescending(p => p.InWarehouse).ToListAsync();
        }

        
    }
}
