using Microsoft.AspNetCore.Mvc;
using Products.API.Models;
using Products.API.Exceptions;

namespace Products.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetProducts([FromQuery] string? categoria, [FromQuery] string? nombre)
        {
            return Ok(new List<Product>());
        }

        [HttpGet("{id}")]
        public IActionResult GetProductById(Guid id)
        {
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] Product product)
        {
            product.Id = Guid.NewGuid();
            product.FechaCreacion = DateTime.UtcNow;
            return Created($"/api/products/{product.Id}", product);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(Guid id, [FromBody] Product product)
        {
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(Guid id)
        {
            return NoContent();
        }
    }
}