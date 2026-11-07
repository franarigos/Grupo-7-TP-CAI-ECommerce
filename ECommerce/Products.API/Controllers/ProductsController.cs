
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Services;

namespace Products.API.Controllers
{
    /// <summary>
    /// API para gestionar productos.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    [Tags("Productos")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Lista productos con filtros opcionales.
        /// </summary>
        /// <param name="categoria">Categoría del producto.</param>
        /// <param name="nombre">Nombre del producto.</param>
        /// <response code="200">Lista de productos.</response>
        /// <response code="500">PRD-005: Error interno.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? categoria,
            [FromQuery] string? nombre)
        {
            List<ProductResponse> response =
                await _productService.GetProductsAsync(
                    categoria,
                    nombre);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene un producto por ID.
        /// </summary>
        /// <param name="id">Identificador del producto.</param>
        /// <response code="200">Producto encontrado.</response>
        /// <response code="404">PRD-001: Producto no encontrado.</response>
        /// <response code="500">PRD-005: Error interno.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            ProductResponse response =
                await _productService.GetProductByIdAsync(id);

            return Ok(response);
        }

        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        /// {
        ///   "nombre": "Notebook Dell XPS 15",
        ///   "descripcion": "Laptop 15 pulgadas",
        ///   "precio": 1500,
        ///   "stock": 10,
        ///   "categoria": "Electrónica"
        /// }
        ///
        /// Ejemplo de respuesta 409:
        ///
        /// {
        ///   "errorCode": "PRD-003",
        ///   "errorMessage": "Ya existe un producto con ese nombre en la categoría."
        /// }
        /// </remarks>
        /// <param name="request">Datos del producto.</param>
        /// <response code="201">Producto creado.</response>
        /// <response code="400">PRD-002: Datos inválidos.</response>
        /// <response code="409">PRD-003: Producto duplicado.</response>
        /// <response code="500">PRD-005: Error interno.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponse), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> CreateProduct(
            [FromBody] ProductRequest request)
        {
            ProductResponse response =
                await _productService.CreateProductAsync(request);

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = response.Id },
                response);
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="id">Identificador del producto.</param>
        /// <param name="request">Datos actualizados.</param>
        /// <response code="200">Producto actualizado.</response>
        /// <response code="400">PRD-002: Datos inválidos.</response>
        /// <response code="404">PRD-001: Producto no encontrado.</response>
        /// <response code="500">PRD-005: Error interno.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> UpdateProduct(
            Guid id,
            [FromBody] ProductRequest request)
        {
            ProductResponse response =
                await _productService.UpdateProductAsync(id, request);

            return Ok(response);
        }

        /// <summary>
        /// Elimina un producto sin órdenes activas.
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta 409:
        ///
        /// {
        ///   "errorCode": "PRD-004",
        ///   "errorMessage": "El producto tiene órdenes activas y no puede eliminarse."
        /// }
        /// </remarks>
        /// <param name="id">Identificador del producto.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <response code="204">Producto eliminado.</response>
        /// <response code="404">PRD-001: Producto no encontrado.</response>
        /// <response code="409">PRD-004: Producto con órdenes activas.</response>
        /// <response code="500">PRD-005: Error interno.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> DeleteProduct(
            Guid id,
            CancellationToken cancellationToken)
        {
            await _productService.DeleteProductAsync(
                id,
                cancellationToken);

            return NoContent();
        }
    }
}
