using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoesApi.Data;
using ShoesApi.Models;
using System.Text.Json;

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
                [FromQuery] string? searchTerm = null,
                [FromQuery] int? selectSupplierID = null)
        {
            var query = _context.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (selectSupplierID.HasValue)
            {
                query = query.Where(p => p.SupplierId == selectSupplierID.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var words = searchTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    var s = word.ToLower();
                    query = query.Where(p =>
                        p.ProductName.ToLower().Contains(s) ||
                        (p.Description != null && p.Description.ToLower().Contains(s)) ||
                        p.Category.ToLower().Contains(s) ||
                        p.Manufacturer.ManufacturerName.ToLower().Contains(s) ||
                        p.Supplier.SupplierName.ToLower().Contains(s) ||
                        p.UnitOfMeasurement.ToLower().Contains(s));
                }
            }

            query = query.Distinct();

            if (sortCount)
                return await query.OrderBy(p => p.InWarehouse).ToListAsync();
            else
                return await query.OrderByDescending(p => p.InWarehouse).ToListAsync();
        }
        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(
            [FromForm] string product,
            IFormFile? image)
        {
            try
            {
                var newProduct = JsonSerializer.Deserialize<Product>(product, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                if (newProduct == null)
                    return BadRequest("Неверные данные товара");

                // Генерация нового ID
                newProduct.ProductId = Guid.NewGuid().ToString();

                // Сохранение изображения
                if (image != null)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                    var ext = Path.GetExtension(image.FileName).ToLower();
                    if (!allowedExtensions.Contains(ext))
                        return BadRequest("Неподдерживаемый формат изображения");

                    string fileName = $"{Guid.NewGuid()}{ext}";
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    newProduct.Photo = fileName;
                }

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                await _context.Entry(newProduct).Reference(p => p.Manufacturer).LoadAsync();
                await _context.Entry(newProduct).Reference(p => p.Supplier).LoadAsync();

                return CreatedAtAction(nameof(GetProducts), new { id = newProduct.ProductId }, newProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        // PUT: api/Products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(string id,
            [FromForm] string product,
            IFormFile? image)
        {
            try
            {
                var existing = await _context.Products.FindAsync(id);
                if (existing == null)
                    return NotFound();

                var updatedData = JsonSerializer.Deserialize<Product>(product, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                if (updatedData == null)
                    return BadRequest("Неверные данные товара");

                // обновление полей
                existing.ProductName = updatedData.ProductName;
                existing.UnitOfMeasurement = updatedData.UnitOfMeasurement;
                existing.Price = updatedData.Price;
                existing.SupplierId = updatedData.SupplierId;
                existing.ManufacturerId = updatedData.ManufacturerId;
                existing.Category = updatedData.Category;
                existing.Current = updatedData.Current;
                existing.InWarehouse = updatedData.InWarehouse;
                existing.Description = updatedData.Description;

                // Обработка изображения
                if (image != null)
                {
                    // Удаляем старое фото
                    if (!string.IsNullOrEmpty(existing.Photo))
                    {
                        string oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", existing.Photo);
                    }

                    // Проверка расширения
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
                    var ext = Path.GetExtension(image.FileName).ToLower();
                    if (!allowedExtensions.Contains(ext))
                        return BadRequest("Неподдерживаемый формат изображения");

                    string fileName = $"{Guid.NewGuid()}{ext}";
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    existing.Photo = fileName;
                }

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка сервера: {ex.Message}");
            }
        }

        // DELETE: api/Products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _context.Products
                .Include(p => p.OrdersProducts)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null)
                return NotFound();

            // Удаляем файл изображения
            if (!string.IsNullOrEmpty(product.Photo))
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", product.Photo);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
