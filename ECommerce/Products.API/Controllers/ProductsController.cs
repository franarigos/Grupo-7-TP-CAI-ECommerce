using Microsoft.AspNetCore.Mvc;

namespace Products.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        // GET /api/products
        [HttpGet]
        public IActionResult GetProducts([FromQuery] string categoria, [FromQuery] string nombre)
        {
            // Retornamos un 200 OK vacío por el momento
            return Ok();
        }

        // GET /api/products/{id}
        [HttpGet("{id}")]
        public IActionResult GetProductById(Guid id)
        {
            // Simulamos que el producto no existe para disparar el error PRD-001
            bool productoEncontrado = false;

            if (!productoEncontrado)
            {
                // Usaremos la excepción personalizada que armaremos en el próximo paso
                throw new Exception("PRD-001: Producto no encontrado.");
            }

            return Ok();
        }
    }
}