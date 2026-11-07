using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    /// <summary>
    /// API para gestionar ordenes de compra.
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    [Tags("Orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        /// <summary>
        /// Lista las ordenes con filtro opcional por usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <response code="200">Lista obtenida correctamente.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<OrderResponse>), 200)]
        [ProducesResponseType(500)]
        public IActionResult ObtenerOrdenes(
            [FromQuery] Guid? usuarioId)
        {
            List<OrderResponse> response =
                _ordersService.ObtenerOrdenes(usuarioId);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene una orden por su identificador.
        /// </summary>
        /// <param name="id">Identificador de la orden.</param>
        /// <response code="200">Orden encontrada.</response>
        /// <response code="404">ORD-001: Orden no encontrada.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult ObtenerOrdenPorId(Guid id)
        {
            OrderResponse response =
                _ordersService.ObtenerOrdenPorId(id);

            return Ok(response);
        }

        /// <summary>
        /// Crea una nueva orden de compra.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        /// {
        ///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///   "items": [
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 2
        ///     }
        ///   ]
        /// }
        /// </remarks>
        /// <param name="request">Datos de la orden.</param>
        /// <param name="cancellationToken">Token de cancelacion.</param>
        /// <response code="201">Orden creada correctamente.</response>
        /// <response code="400">ORD-002: Datos invalidos.</response>
        /// <response code="404">ORD-003: Usuario inexistente; ORD-004: Producto inexistente.</response>
        /// <response code="409">Conflicto.</response>
        /// <response code="422">ORD-005: Stock insuficiente.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(422)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CrearOrden(
            [FromBody] CreateOrderRequest request,
            CancellationToken cancellationToken)
        {
            OrderResponse response =
                await _ordersService.CrearOrdenAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(ObtenerOrdenPorId),
                new { id = response.Id },
                response);
        }

        /// <summary>
        /// Actualiza el estado de una orden.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        /// { "estado": "Confirmada" }
        /// </remarks>
        /// <param name="id">Identificador de la orden.</param>
        /// <param name="request">Nuevo estado.</param>
        /// <response code="200">Estado actualizado.</response>
        /// <response code="400">ORD-002: Estado invalido.</response>
        /// <response code="404">ORD-001: Orden no encontrada.</response>
        /// <response code="409">ORD-006: Transicion no permitida.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult ActualizarEstado(
            Guid id,
            [FromBody] UpdateOrderStatusRequest request)
        {
            OrderResponse response =
                _ordersService.ActualizarEstado(id, request);

            return Ok(response);
        }
    }
}