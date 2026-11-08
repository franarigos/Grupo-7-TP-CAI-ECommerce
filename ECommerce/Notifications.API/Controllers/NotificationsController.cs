using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers
{
    /// <summary>
    /// API para registrar y consultar notificaciones.
    /// </summary>
    [ApiController]
    [Route("api/notifications")]
    [Tags("Notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _service;

        /// <summary>
        /// Inicializa el controlador de notificaciones.
        /// </summary>
        /// <param name="service">Servicio de notificaciones.</param>
        public NotificationsController(
            NotificationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Registra y simula el envío de una notificación.
        /// </summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// {
        ///   "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "mensaje": "Su orden fue confirmada.",
        ///   "tipo": "Email"
        /// }
        /// </code>
        /// Respuesta 201:
        /// <code>
        /// {
        ///   "id": "11112222-3333-4444-5555-666677778888",
        ///   "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "mensaje": "Su orden fue confirmada.",
        ///   "tipo": "Email",
        ///   "estado": "Enviada",
        ///   "fechaEnvio": "2026-11-07T12:00:00Z"
        /// }
        /// </code>
        /// Respuesta 404:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "El recurso solicitado no fue encontrado.",
        ///   "instance": "/api/notifications/send",
        ///   "errorCode": "NTF-001",
        ///   "errorMessage": "El usuario destinatario no fue encontrado.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">Datos de la notificación.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <response code="415">NTF-HTTP-415: Formato de contenido no admitido.</response>
        /// <response code="201">Notificación registrada.</response>
        /// <response code="400">NTF-002: Datos inválidos.</response>
        /// <response code="404">NTF-001: Usuario no encontrado.</response>
        /// <response code="500">NTF-004: Error interno.</response>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ApiErrorResponse), 415)]
        [ProducesResponseType(typeof(NotificationResponse), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> Send(
            [FromBody] SendNotificationRequest request,
            CancellationToken cancellationToken)
        {
            NotificationResponse response =
                await _service.SendAsync(
                    request,
                    cancellationToken);

            return StatusCode(201, response);
        }

        /// <summary>
        /// Lista las notificaciones de un usuario.
        /// </summary>
        /// <remarks>
        /// Respuesta 200:
        /// <code>
        /// [
        ///   {
        ///     "id": "11112222-3333-4444-5555-666677778888",
        ///     "usuarioId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///     "mensaje": "Su orden fue confirmada.",
        ///     "tipo": "Email",
        ///     "estado": "Enviada",
        ///     "fechaEnvio": "2026-11-07T12:00:00Z"
        ///   }
        /// ]
        /// </code>
        /// Respuesta 404:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "El recurso solicitado no fue encontrado.",
        ///   "instance": "/api/notifications/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "errorCode": "NTF-003",
        ///   "errorMessage": "No se encontraron notificaciones para el usuario.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="userId">Identificador del usuario en formato GUID.</param>
        /// <response code="200">Lista de notificaciones.</response>
        /// <response code="400">NTF-002: Identificador inválido.</response>
        /// <response code="404">NTF-003: Sin notificaciones.</response>
        /// <response code="500">NTF-004: Error interno.</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(List<NotificationResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> GetByUser(Guid userId)
        {
            List<NotificationResponse> response =
                await _service.GetByUserAsync(userId);

            return Ok(response);
        }
    }
}