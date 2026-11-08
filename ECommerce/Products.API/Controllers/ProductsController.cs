using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs;
using Products.API.Services;

namespace Products.API.Controllers
{
    /// <summary>Gestiona los productos.</summary>
    [ApiController]
    [Route("api/products")]
    [Tags("Productos")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        /// <summary>Inicializa el controlador.</summary>
        /// <param name="productService">Servicio de productos.</param>
        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        /// <summary>Lista productos con filtros opcionales.</summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// GET /api/products?categoria=Electrónica&amp;nombre=Notebook
        /// </code>
        /// Respuesta 200 sin coincidencias:
        /// <code>
        /// []
        /// </code>
        /// Respuesta 500:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        ///   "title": "Internal Server Error",
        ///   "status": 500,
        ///   "detail": "Ocurrió un error inesperado en el servidor.",
        ///   "instance": "/api/products",
        ///   "errorCode": "PRD-005",
        ///   "errorMessage": "Error interno al procesar el producto.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="categoria">Categoría opcional.</param>
        /// <param name="nombre">Texto opcional del nombre.</param>
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

        /// <summary>Obtiene un producto por ID.</summary>
        /// <remarks>
        /// Respuesta 200:
        /// <code>
        /// {
        ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "nombre": "Notebook Dell XPS 15",
        ///   "descripcion": "Laptop de 15 pulgadas",
        ///   "precio": 1500,
        ///   "stock": 10,
        ///   "categoria": "Electrónica",
        ///   "fechaCreacion": "2026-11-07T12:00:00Z"
        /// }
        /// </code>
        /// Respuesta 404:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "El recurso solicitado no fue encontrado.",
        ///   "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "errorCode": "PRD-001",
        ///   "errorMessage": "Producto no encontrado.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">Identificador del producto en formato GUID.</param>
        /// <response code="200">Producto encontrado.</response>
        /// <response code="400">PRD-002: Identificador inválido.</response>
        /// <response code="404">PRD-001: Producto no encontrado.</response>
        /// <response code="500">PRD-005: Error interno.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            ProductResponse response =
                await _productService.GetProductByIdAsync(id);

            return Ok(response);
        }

        /// <summary>Crea un producto.</summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// {
        ///   "nombre": "Notebook Dell XPS 15",
        ///   "descripcion": "Laptop de 15 pulgadas",
        ///   "precio": 1500,
        ///   "stock": 10,
        ///   "categoria": "Electrónica"
        /// }
        /// </code>
        /// Respuesta 201:
        /// <code>
        /// {
        ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "nombre": "Notebook Dell XPS 15",
        ///   "descripcion": "Laptop de 15 pulgadas",
        ///   "precio": 1500,
        ///   "stock": 10,
        ///   "categoria": "Electrónica",
        ///   "fechaCreacion": "2026-11-07T12:00:00Z"
        /// }
        /// </code>
        /// Respuesta 409:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
        ///   "title": "Conflict",
        ///   "status": 409,
        ///   "detail": "Ya existe un recurso con esos datos.",
        ///   "instance": "/api/products",
        ///   "errorCode": "PRD-003",
        ///   "errorMessage": "Ya existe un producto con ese nombre en la categoría 'Electrónica'.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">Datos del producto.</param>
        /// <response code="201">Producto creado.</response>
        /// <response code="400">PRD-002: Datos inválidos.</response>
        /// <response code="409">PRD-003: Producto duplicado.</response>
        /// <response code="415">PRD-HTTP-415: Formato no admitido.</response>
        /// <response code="500">PRD-005: Error interno.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponse), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        [ProducesResponseType(typeof(ApiErrorResponse), 415)]
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

        /// <summary>Actualiza un producto.</summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// {
        ///   "nombre": "Notebook Dell XPS 15",
        ///   "descripcion": "Laptop de 15 pulgadas",
        ///   "precio": 1500,
        ///   "stock": 10,
        ///   "categoria": "Electrónica"
        /// }
        /// </code>
        /// Respuesta 200:
        /// <code>
        /// {
        ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "nombre": "Notebook Dell XPS 15",
        ///   "descripcion": "Laptop de 15 pulgadas",
        ///   "precio": 1500,
        ///   "stock": 10,
        ///   "categoria": "Electrónica",
        ///   "fechaCreacion": "2026-11-07T12:00:00Z"
        /// }
        /// </code>
        /// Respuesta 400:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ///   "title": "Bad Request",
        ///   "status": 400,
        ///   "detail": "Los datos del producto son inválidos.",
        ///   "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "errorCode": "PRD-002",
        ///   "errorMessage": "Los datos del producto son inválidos.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">Identificador del producto en formato GUID.</param>
        /// <param name="request">Datos actualizados.</param>
        /// <response code="200">Producto actualizado.</response>
        /// <response code="400">PRD-002: Datos o identificador inválidos.</response>
        /// <response code="404">PRD-001: Producto no encontrado.</response>
        /// <response code="415">PRD-HTTP-415: Formato no admitido.</response>
        /// <response code="500">PRD-005: Error interno.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 415)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> UpdateProduct(
            Guid id,
            [FromBody] ProductRequest request)
        {
            ProductResponse response =
                await _productService.UpdateProductAsync(id, request);

            return Ok(response);
        }

        /// <summary>Elimina un producto sin órdenes activas.</summary>
        /// <remarks>
        /// Respuesta 204: sin cuerpo.
        ///
        /// Respuesta 409:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
        ///   "title": "Conflict",
        ///   "status": 409,
        ///   "detail": "No se puede eliminar el recurso.",
        ///   "instance": "/api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "errorCode": "PRD-004",
        ///   "errorMessage": "El producto tiene órdenes activas y no puede eliminarse.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">Identificador del producto en formato GUID.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <response code="204">Producto eliminado; sin cuerpo.</response>
        /// <response code="400">PRD-002: Identificador inválido.</response>
        /// <response code="404">PRD-001: Producto no encontrado.</response>
        /// <response code="409">PRD-004: Producto con órdenes activas.</response>
        /// <response code="500">PRD-005: Error interno.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
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