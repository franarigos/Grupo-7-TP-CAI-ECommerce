
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
    /// API para gestionar órdenes de compra.
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
        /// Lista órdenes con filtro opcional por usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <response code="200">Lista de órdenes.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<OrderResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> ObtenerOrdenes(
            [FromQuery] Guid? usuarioId)
        {
            List<OrderResponse> response =
                await _ordersService.ObtenerOrdenesAsync(usuarioId);

            return Ok(response);
        }

        /// <summary>
        /// Obtiene una orden por identificador.
        /// </summary>
        /// <param name="id">Identificador de la orden.</param>
        /// <response code="200">Orden encontrada.</response>
        /// <response code="404">ORD-001: Orden no encontrada.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> ObtenerOrdenPorId(Guid id)
        {
            OrderResponse response =
                await _ordersService.ObtenerOrdenPorIdAsync(id);

            return Ok(response);
        }

        /// <summary>
        /// Crea una nueva orden.
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
        ///
        /// Ejemplo de respuesta de error:
        ///
        /// {
        ///   "errorCode": "ORD-005",
        ///   "errorMessage": "Stock insuficiente para el producto."
        /// }
        /// </remarks>
        /// <param name="request">Datos de la orden.</param>
        /// <param name="cancellationToken">Cancelación de la solicitud.</param>
        /// <response code="201">Orden creada.</response>
        /// <response code="400">ORD-002: Datos inválidos.</response>
        /// <response code="404">ORD-003: Usuario inexistente; ORD-004: Producto inexistente.</response>
        /// <response code="422">ORD-005: Stock insuficiente.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 422)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
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
        /// {
        ///   "estado": "Confirmada"
        /// }
        ///
        /// Ejemplo de respuesta de error:
        ///
        /// {
        ///   "errorCode": "ORD-006",
        ///   "errorMessage": "El estado de la orden no puede ser modificado."
        /// }
        /// </remarks>
        /// <param name="id">Identificador de la orden.</param>
        /// <param name="request">Estado nuevo.</param>
        /// <response code="200">Orden actualizada.</response>
        /// <response code="400">ORD-002: Estado inválido.</response>
        /// <response code="404">ORD-001: Orden no encontrada.</response>
        /// <response code="409">ORD-006: Transición inválida.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> ActualizarEstado(
            Guid id,
            [FromBody] UpdateOrderStatusRequest request)
        {
            OrderResponse response =
                await _ordersService.ActualizarEstadoAsync(
                    id,
                    request);

            return Ok(response);
        }
    }
}
