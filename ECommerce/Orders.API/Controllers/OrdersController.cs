using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    /// <summary>Gestiona las órdenes de compra.</summary>
    [ApiController]
    [Route("api/orders")]
    [Tags("Orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;

        /// <summary>Inicializa el controlador.</summary>
        /// <param name="ordersService">Servicio de órdenes.</param>
        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        /// <summary>Lista órdenes con filtro opcional por usuario.</summary>
        /// <remarks>
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
        ///   "detail": "Ocurrio un error inesperado en el servidor.",
        ///   "instance": "/api/orders",
        ///   "errorCode": "ORD-007",
        ///   "errorMessage": "Error interno al procesar la orden.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="usuarioId">Identificador opcional del usuario en formato GUID.</param>
        /// <response code="200">Lista de órdenes.</response>
        /// <response code="400">ORD-002: Formato del filtro inválido.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<OrderResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> ObtenerOrdenes(
            [FromQuery] Guid? usuarioId)
        {
            List<OrderResponse> response =
                await _ordersService.ObtenerOrdenesAsync(usuarioId);

            return Ok(response);
        }

        /// <summary>Obtiene una orden por ID.</summary>
        /// <remarks>
        /// Respuesta 200:
        /// <code>
        /// {
        ///   "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        ///   "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "items": [
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 2,
        ///       "precioUnitario": 1500
        ///     }
        ///   ],
        ///   "total": 3000,
        ///   "estado": "Pendiente",
        ///   "fechaCreacion": "2026-11-07T12:00:00Z",
        ///   "fechaActualizacion": null
        /// }
        /// </code>
        /// Respuesta 404:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "El recurso solicitado no fue encontrado.",
        ///   "instance": "/api/orders/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        ///   "errorCode": "ORD-001",
        ///   "errorMessage": "Orden no encontrada.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">Identificador de la orden en formato GUID.</param>
        /// <response code="200">Orden encontrada.</response>
        /// <response code="400">ORD-002: Formato de identificador inválido.</response>
        /// <response code="404">ORD-001: Orden no encontrada.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> ObtenerOrdenPorId(Guid id)
        {
            OrderResponse response =
                await _ordersService.ObtenerOrdenPorIdAsync(id);

            return Ok(response);
        }

        /// <summary>Crea una orden.</summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// {
        ///   "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "items": [
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 2
        ///     }
        ///   ]
        /// }
        /// </code>
        /// Respuesta 201:
        /// <code>
        /// {
        ///   "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        ///   "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "items": [
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 2,
        ///       "precioUnitario": 1500
        ///     }
        ///   ],
        ///   "total": 3000,
        ///   "estado": "Pendiente",
        ///   "fechaCreacion": "2026-11-07T12:00:00Z",
        ///   "fechaActualizacion": null
        /// }
        /// </code>
        /// Respuesta 422:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc4918#section-11.2",
        ///   "title": "Unprocessable Entity",
        ///   "status": 422,
        ///   "detail": "No se puede procesar la solicitud.",
        ///   "instance": "/api/orders",
        ///   "errorCode": "ORD-005",
        ///   "errorMessage": "Stock insuficiente para 'Notebook Dell XPS 15'. Disponible: 2, solicitado: 5.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">Usuario y productos con cantidades mayores a cero.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <response code="201">Orden creada.</response>
        /// <response code="400">ORD-002: Datos inválidos o lista de productos vacía.</response>
        /// <response code="404">ORD-003: Usuario inexistente; ORD-004: Producto inexistente.</response>
        /// <response code="415">ORD-HTTP-415: Formato de contenido no admitido.</response>
        /// <response code="422">ORD-005: Stock insuficiente.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 415)]
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

        /// <summary>Actualiza el estado de una orden.</summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// {
        ///   "estado": "Confirmada"
        /// }
        /// </code>
        /// Respuesta 200:
        /// <code>
        /// {
        ///   "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        ///   "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "items": [
        ///     {
        ///       "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///       "cantidad": 2,
        ///       "precioUnitario": 1500
        ///     }
        ///   ],
        ///   "total": 3000,
        ///   "estado": "Confirmada",
        ///   "fechaCreacion": "2026-11-07T12:00:00Z",
        ///   "fechaActualizacion": "2026-11-07T12:30:00Z"
        /// }
        /// </code>
        /// Respuesta 409:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
        ///   "title": "Conflict",
        ///   "status": 409,
        ///   "detail": "No se puede modificar el estado.",
        ///   "instance": "/api/orders/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb/status",
        ///   "errorCode": "ORD-006",
        ///   "errorMessage": "Una orden en estado 'Entregada' no puede pasar a 'Pendiente'.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">Identificador de la orden en formato GUID.</param>
        /// <param name="request">Nuevo estado de la orden.</param>
        /// <response code="200">Orden actualizada.</response>
        /// <response code="400">ORD-002: Estado o identificador inválidos.</response>
        /// <response code="404">ORD-001: Orden no encontrada.</response>
        /// <response code="409">ORD-006: Transición de estado inválida.</response>
        /// <response code="415">ORD-HTTP-415: Formato de contenido no admitido.</response>
        /// <response code="500">ORD-007: Error interno.</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        [ProducesResponseType(typeof(ApiErrorResponse), 415)]
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