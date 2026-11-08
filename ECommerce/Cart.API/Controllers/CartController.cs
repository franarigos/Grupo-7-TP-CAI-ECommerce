using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using global::Cart.API.DTOs;
using global::Cart.API.Services;

namespace Cart.API.Controllers
{
    /// <summary>Gestiona los carritos de compras.</summary>
    [ApiController]
    [Route("api/cart")]
    [Tags("Carrito")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        /// <summary>Inicializa el controlador.</summary>
        /// <param name="cartService">Servicio del carrito.</param>
        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>Obtiene el carrito de un usuario.</summary>
        /// <remarks>
        /// Respuesta 200:
        /// <code>
        /// {
        ///   "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "items": [
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 2
        ///     }
        ///   ],
        ///   "fechaActualizacion": "2026-11-07T12:00:00Z"
        /// }
        /// </code>
        /// Respuesta 404:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "El recurso solicitado no fue encontrado.",
        ///   "instance": "/api/cart/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "errorCode": "CRT-001",
        ///   "errorMessage": "Carrito no encontrado.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="userId">Identificador del usuario en formato GUID.</param>
        /// <response code="200">Carrito encontrado.</response>
        /// <response code="400">CRT-004: Formato de identificador inválido.</response>
        /// <response code="404">CRT-001: Carrito no encontrado.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            CartResponse response =
                await _cartService.GetCartAsync(userId);

            return Ok(response);
        }

        /// <summary>Agrega un producto al carrito.</summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// {
        ///   "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "cantidad": 2
        /// }
        /// </code>
        /// Respuesta 200:
        /// <code>
        /// {
        ///   "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "items": [
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 2
        ///     }
        ///   ],
        ///   "fechaActualizacion": "2026-11-07T12:00:00Z"
        /// }
        /// </code>
        /// Respuesta 422:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
        ///   "title": "Unprocessable Entity",
        ///   "status": 422,
        ///   "detail": "No se puede procesar la solicitud.",
        ///   "instance": "/api/cart/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/items",
        ///   "errorCode": "CRT-003",
        ///   "errorMessage": "Stock insuficiente. Disponible: 1, solicitado: 2.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="userId">Identificador del usuario en formato GUID.</param>
        /// <param name="request">Producto y cantidad mayor a cero.</param>
        /// <response code="200">Producto agregado al carrito.</response>
        /// <response code="400">CRT-004: Cantidad o formato inválidos.</response>
        /// <response code="404">CRT-002: Producto no encontrado.</response>
        /// <response code="415">CRT-HTTP-415: Formato de contenido no admitido.</response>
        /// <response code="422">CRT-003: Stock insuficiente.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpPost("{userId}/items")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 415)]
        [ProducesResponseType(typeof(ApiErrorResponse), 422)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> AgregarItem(
            Guid userId,
            [FromBody] AddCartItemRequest request)
        {
            CartResponse response =
                await _cartService.AgregarItemAsync(
                    userId,
                    request);

            return Ok(response);
        }

        /// <summary>Actualiza la cantidad de un producto.</summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// {
        ///   "cantidad": 4
        /// }
        /// </code>
        /// Respuesta 200:
        /// <code>
        /// {
        ///   "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "items": [
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 4
        ///     }
        ///   ],
        ///   "fechaActualizacion": "2026-11-07T12:00:00Z"
        /// }
        /// </code>
        /// Respuesta 400:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ///   "title": "Bad Request",
        ///   "status": 400,
        ///   "detail": "Los datos enviados no son válidos.",
        ///   "instance": "/api/cart/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/items/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "errorCode": "CRT-004",
        ///   "errorMessage": "La cantidad debe ser mayor a cero.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="userId">Identificador del usuario en formato GUID.</param>
        /// <param name="productId">Identificador del producto en formato GUID.</param>
        /// <param name="request">Nueva cantidad mayor a cero.</param>
        /// <response code="200">Cantidad actualizada.</response>
        /// <response code="400">CRT-004: Cantidad o formato inválidos.</response>
        /// <response code="404">CRT-001: Carrito inexistente; CRT-002: Producto no encontrado.</response>
        /// <response code="415">CRT-HTTP-415: Formato de contenido no admitido.</response>
        /// <response code="422">CRT-003: Stock insuficiente.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpPut("{userId}/items/{productId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 415)]
        [ProducesResponseType(typeof(ApiErrorResponse), 422)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> UpdateItemQuantity(
            Guid userId,
            Guid productId,
            [FromBody] UpdateCartItemRequest request)
        {
            CartResponse response =
                await _cartService.UpdateItemQuantityAsync(
                    userId,
                    productId,
                    request.Cantidad);

            return Ok(response);
        }

        /// <summary>Quita un producto del carrito.</summary>
        /// <remarks>
        /// Respuesta 204: sin cuerpo.
        ///
        /// Respuesta 404:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "El recurso solicitado no fue encontrado.",
        ///   "instance": "/api/cart/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/items/3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "errorCode": "CRT-001",
        ///   "errorMessage": "Carrito no encontrado.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="userId">Identificador del usuario en formato GUID.</param>
        /// <param name="productId">Identificador del producto en formato GUID.</param>
        /// <response code="204">Producto quitado; respuesta sin cuerpo.</response>
        /// <response code="400">CRT-004: Formato de identificador inválido.</response>
        /// <response code="404">CRT-001: Carrito no encontrado.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> RemoveItem(
            Guid userId,
            Guid productId)
        {
            await _cartService.RemoveItemAsync(
                userId,
                productId);

            return NoContent();
        }

        /// <summary>Vacía el carrito.</summary>
        /// <remarks>
        /// Respuesta 204: sin cuerpo.
        ///
        /// Respuesta 404:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "El recurso solicitado no fue encontrado.",
        ///   "instance": "/api/cart/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "errorCode": "CRT-001",
        ///   "errorMessage": "Carrito no encontrado.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="userId">Identificador del usuario en formato GUID.</param>
        /// <response code="204">Carrito vaciado; respuesta sin cuerpo.</response>
        /// <response code="400">CRT-004: Formato de identificador inválido.</response>
        /// <response code="404">CRT-001: Carrito no encontrado.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpDelete("{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _cartService.ClearCartAsync(userId);

            return NoContent();
        }
    }
}