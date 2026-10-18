using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Products.API.Services;
using Products.API.DTOs;

namespace Products.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Productos")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Listar productos (con filtros opcionales por categoría y nombre)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductResponse>), 200)]
        [ProducesResponseType(500)]
        public IActionResult GetProducts([FromQuery] string? categoria, [FromQuery] string? nombre)
        {
            var response = _productService.GetProducts(categoria, nombre);
            return Ok(response);
        }

        /// <summary>
        /// Obtener producto por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult GetProduct(Guid id)
        {
            var response = _productService.GetProductById(id);
            return Ok(response);
        }

        /// <summary>
        /// Crear nuevo producto
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult CreateProduct([FromBody] ProductRequest request)
        {
            var response = _productService.CreateProduct(request);
            return CreatedAtAction(nameof(GetProduct), new { id = response.Id }, response);
        }

        /// <summary>
        /// Actualizar producto existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult UpdateProduct(Guid id, [FromBody] ProductRequest request)
        {
            var response = _productService.UpdateProduct(id, request);
            return Ok(response);
        }

        /// <summary>
        /// Eliminar producto
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult DeleteProduct(Guid id)
        {
            _productService.DeleteProduct(id);
            return NoContent();
        }
    }
}