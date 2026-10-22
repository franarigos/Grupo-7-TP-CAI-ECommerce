using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    /// <summary>
    /// API para la gestion de ordenes de compra.
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        /// <summary>Lista las ordenes (filtro opcional por usuario).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<OrderResponse>), 200)]
        [ProducesResponseType(500)]
        public IActionResult ObtenerOrdenes([FromQuery] Guid? usuarioId)
        {
            var response = _ordersService.ObtenerOrdenes(usuarioId);
            return Ok(response);
        }

        /// <summary>Obtiene el detalle de una orden por ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult ObtenerOrdenPorId(Guid id)
        {
            var response = _ordersService.ObtenerOrdenPorId(id);
            return Ok(response);
        }

        /// <summary>Crea una nueva orden.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public IActionResult CrearOrden([FromBody] CreateOrderRequest request)
        {
            var response = _ordersService.CrearOrden(request);

            // Retorna un 201 Created y en los headers indica la URL para consultar la orden creada
            return CreatedAtAction(nameof(ObtenerOrdenPorId), new { id = response.Id }, response);
        }

        /// <summary>Actualiza el estado de una orden.</summary>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult ActualizarEstado(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var response = _ordersService.ActualizarEstado(id, request);
            return Ok(response);
        }
    }
}
