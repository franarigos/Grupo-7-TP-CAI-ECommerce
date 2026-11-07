using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using global::Cart.API.DTOs;
using global::Cart.API.Services;

namespace Cart.API.Controllers
{
    /// <summary>
    /// API para gestionar carritos de compras.
    /// </summary>
    [ApiController]
    [Route("api/cart")]
    [Tags("Carrito")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Obtiene el carrito de un usuario.
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta 200:
        ///
        /// {
        ///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///   "items": [
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 2
        ///     }
        ///   ],
        ///   "fechaActualizacion": "2026-11-07T10:00:00Z"
        /// }
        /// </remarks>
        /// <param name="userId">Identificador del usuario.</param>
        /// <response code="200">Carrito obtenido correctamente.</response>
        /// <response code="404">CRT-001: Carrito no encontrado.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> GetCart(Guid userId)
        {
            CartResponse response =
                await _cartService.GetCartAsync(userId);

            return Ok(response);
        }

        /// <summary>
        /// Agrega un producto al carrito.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        /// {
        ///   "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "cantidad": 2
        /// }
        ///
        /// Ejemplo de respuesta 422:
        ///
        /// {
        ///   "errorCode": "CRT-003",
        ///   "errorMessage": "Stock insuficiente. Disponible: 1, solicitado: 2."
        /// }
        /// </remarks>
        /// <param name="userId">Identificador del usuario.</param>
        /// <param name="request">Producto y cantidad.</param>
        /// <response code="200">Producto agregado al carrito.</response>
        /// <response code="400">CRT-004: Cantidad inválida.</response>
        /// <response code="404">CRT-002: Producto no encontrado.</response>
        /// <response code="422">CRT-003: Stock insuficiente.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpPost("{userId}/items")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
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

        /// <summary>
        /// Actualiza la cantidad de un producto del carrito.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        /// {
        ///   "cantidad": 4
        /// }
        ///
        /// Ejemplo de respuesta 400:
        ///
        /// {
        ///   "errorCode": "CRT-004",
        ///   "errorMessage": "Cantidad inválida."
        /// }
        /// </remarks>
        /// <param name="userId">Identificador del usuario.</param>
        /// <param name="productId">Identificador del producto.</param>
        /// <param name="request">Nueva cantidad.</param>
        /// <response code="200">Cantidad actualizada.</response>
        /// <response code="400">CRT-004: Cantidad inválida.</response>
        /// <response code="404">CRT-001 o CRT-002: Recurso no encontrado.</response>
        /// <response code="422">CRT-003: Stock insuficiente.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpPut("{userId}/items/{productId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
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

        /// <summary>
        /// Quita un producto del carrito.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <param name="productId">Identificador del producto.</param>
        /// <response code="204">Producto quitado del carrito.</response>
        /// <response code="404">CRT-001: Carrito no encontrado.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(204)]
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

        /// <summary>
        /// Vacía todos los productos del carrito.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <response code="204">Carrito vaciado correctamente.</response>
        /// <response code="404">CRT-001: Carrito no encontrado.</response>
        /// <response code="500">CRT-005: Error interno.</response>
        [HttpDelete("{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _cartService.ClearCartAsync(userId);

            return NoContent();
        }
    }
}