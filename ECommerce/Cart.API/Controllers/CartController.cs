using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Cart.API.DTOs;
using Cart.API.Services;

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
    }
}
