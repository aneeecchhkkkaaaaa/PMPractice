using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoesApi.Data;
using ShoesApi.Models;

namespace ShoesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturersController : ControllerBase
    {
        private readonly is25_practice_shoesContext _context;
        public ManufacturersController(is25_practice_shoesContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Manufacturer>>> GetManufacturers()
        {
            return await _context.Manufacturers.OrderBy(m => m.ManufacturerId).ToListAsync();
        }
    }
}