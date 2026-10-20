using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Cart.API.Services;
using Cart.API.DTOs;

namespace Cart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Carrito")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Obtener carrito del usuario
        /// </summary>
        /// <response code="200">Carrito obtenido exitosamente.</response>
        /// <response code="404">CRT-001</response>
        /// <response code="500">CRT-005</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult GetCart(Guid userId)
        {
            var response = _cartService.GetCart(userId);
            return Ok(response);
        }

        /// <summary>
        /// Agregar producto al carrito
        /// </summary>
        /// <response code="200">Producto agregado exitosamente.</response>
        /// <response code="400">CRT-004</response>
        /// <response code="404">CRT-002</response>
        /// <response code="422">CRT-003</response>
        /// <response code="500">CRT-005</response>
        [HttpPost("{userId}/items")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AgregarItem(Guid userId, [FromBody] AddCartItemRequest request)
        {
            var response = await _cartService.AgregarItemAsync(userId, request);
            return Ok(response);
        }

        /// <summary>
        /// Actualizar cantidad de un item
        /// </summary>
        /// <response code="200">Cantidad actualizada exitosamente.</response>
        /// <response code="400">CRT-004</response>
        /// <response code="404">CRT-001 / CRT-002</response>
        /// <response code="422">CRT-003</response>
        /// <response code="500">CRT-005</response>
        [HttpPut("{userId}/items/{productId}")]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateItemQuantity(Guid userId, Guid productId, [FromBody] UpdateCartItemRequest request)
        {
            var response = await _cartService.UpdateItemQuantityAsync(userId, productId, request.Cantidad);
            return Ok(response);
        }

        /// <summary>
        /// Quitar un producto del carrito
        /// </summary>
        /// <response code="204">Producto eliminado del carrito.</response>
        /// <response code="404">CRT-001</response>
        /// <response code="500">CRT-005</response>
        [HttpDelete("{userId}/items/{productId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult RemoveItem(Guid userId, Guid productId)
        {
            _cartService.RemoveItem(userId, productId);
            return NoContent();
        }

        /// <summary>
        /// Vaciar carrito completo
        /// </summary>
        /// <response code="204">Carrito vaciado exitosamente.</response>
        /// <response code="404">CRT-001</response>
        /// <response code="500">CRT-005</response>
        [HttpDelete("{userId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult ClearCart(Guid userId)
        {
            _cartService.ClearCart(userId);
            return NoContent();
        }
    }
}