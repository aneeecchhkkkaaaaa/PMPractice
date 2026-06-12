using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoesApi.Data;
using ShoesApi.Models;

namespace ShoesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly is25_practice_shoesContext _context;
        public SuppliersController(is25_practice_shoesContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers()
        {
            var query = _context.Suppliers
                .AsQueryable();
            return await query.OrderBy(p => p.SupplierId).ToListAsync();
        }
    }
}
