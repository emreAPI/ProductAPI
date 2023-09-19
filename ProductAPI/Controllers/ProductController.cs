using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly ProductDbContext _context;
        public ProductController(ProductDbContext context) => _context = context;

        [HttpGet("getAll")]
        public async Task<IEnumerable<Product>> Get()
            => await _context.Products.Where(x => !x.IsDeleted).ToListAsync();

        [HttpGet("getById")]
        [ProducesResponseType(typeof(Product),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products.Where(x => !x.IsDeleted).FirstOrDefaultAsync(p => p.Id == id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("update/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, Product product)
        {
            if (id != product.Id) return BadRequest();

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var productToDelete = await _context.Products.FindAsync(id);
            if (productToDelete == null) return NotFound();

            productToDelete.IsDeleted = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPost("search")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromBody] ProductSearchCriteria criteria)
        {
            var query = _context.Products.AsQueryable();
            query = query.Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(criteria.Name))
            {
                query = query.Where(p => p.Name.Contains(criteria.Name));
            }

            if (!string.IsNullOrEmpty(criteria.Code))
            {
                query = query.Where(p => p.Code == criteria.Code);
            }

            if (!string.IsNullOrEmpty(criteria.Brand))
            {
                query = query.Where(p => p.Brand == criteria.Brand);
            }

            if (criteria.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= criteria.MinPrice);
            }

            if (criteria.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= criteria.MaxPrice);
            }

            var products = await query.ToListAsync();
            return Ok(products);
        }


    }
}
