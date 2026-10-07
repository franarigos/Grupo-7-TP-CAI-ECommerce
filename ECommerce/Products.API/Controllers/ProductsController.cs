using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Services;

namespace Products.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public IActionResult GetProducts([FromQuery] string? categoria, [FromQuery] string? nombre)
        {
            return Ok(_productService.GetProducts(categoria, nombre));
        }

        [HttpGet("{id}")]
        public IActionResult GetProductById(Guid id)
        {
            return Ok(_productService.GetProductById(id));
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] ProductRequest request)
        {
            var response = _productService.CreateProduct(request);
            return Created($"/api/products/{response.Id}", response);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProduct(Guid id, [FromBody] ProductRequest request)
        {
            return Ok(_productService.UpdateProduct(id, request));
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(Guid id)
        {
            _productService.DeleteProduct(id);
            return NoContent();
        }
    }
}