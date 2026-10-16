using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Cart.API.DTOs;
using Cart.API.Services;
using Cart.API.Exceptions;

namespace Cart.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpPost("{userId}/items")]
        public async Task<IActionResult> AgregarItem(Guid userId, [FromBody] AddCartItemRequest request)
        {
            var carrito = await _cartService.AgregarItemAsync(userId, request);
            return Ok(carrito);
        }

        [HttpGet("{userId}")]
        public ActionResult<CartResponse> GetCart(Guid userId)
        {
            var carrito = _cartService.GetCart(userId);
            return Ok(carrito);
        }

        [HttpPut("{userId}/items/{productId}")]
        public ActionResult<CartResponse> UpdateItemQuantity(Guid userId, Guid productId, [FromBody] UpdateCartItemRequest request)
        {
            if (request.Cantidad <= 0)
            {
                throw new ValidationException("CRT-004", "Cantidad inválida.");
            }

            var carritoActualizado = _cartService.UpdateItemQuantity(userId, productId, request.Cantidad);
            return Ok(carritoActualizado);
        }

        [HttpDelete("{userId}/items/{productId}")]
        public IActionResult RemoveItem(Guid userId, Guid productId)
        {
            _cartService.RemoveItem(userId, productId);
            return NoContent();
        }

        [HttpDelete("{userId}")]
        public IActionResult ClearCart(Guid userId)
        {
            _cartService.ClearCart(userId);
            return NoContent();
        }
    }
}
