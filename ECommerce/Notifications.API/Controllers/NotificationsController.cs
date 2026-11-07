
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

        public NotificationsController(
            NotificationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Registra y simula el envío de una notificación.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        /// {
        ///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///   "mensaje": "Su orden fue confirmada.",
        ///   "tipo": "Email"
        /// }
        ///
        /// Ejemplo de respuesta 201:
        ///
        /// {
        ///   "id": "11112222-3333-4444-5555-666677778888",
        ///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///   "mensaje": "Su orden fue confirmada.",
        ///   "tipo": "Email",
        ///   "estado": "Enviada",
        ///   "fechaEnvio": "2024-03-10T12:01:00Z"
        /// }
        ///
        /// Ejemplo de respuesta 404:
        ///
        /// {
        ///   "errorCode": "NTF-001",
        ///   "errorMessage": "El usuario destinatario no fue encontrado."
        /// }
        /// </remarks>
        /// <param name="request">Datos de la notificación.</param>
        /// <param name="cancellationToken">Token de cancelación.</param>
        /// <response code="201">Notificación registrada.</response>
        /// <response code="400">NTF-002: Datos inválidos.</response>
        /// <response code="404">NTF-001: Usuario no encontrado.</response>
        /// <response code="500">NTF-004: Error interno.</response>
        [HttpPost("send")]
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
        /// Ejemplo de respuesta 200:
        ///
        /// [
        ///   {
        ///     "id": "11112222-3333-4444-5555-666677778888",
        ///     "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
        ///     "mensaje": "Su orden fue confirmada.",
        ///     "tipo": "Email",
        ///     "estado": "Enviada",
        ///     "fechaEnvio": "2024-03-10T12:01:00Z"
        ///   }
        /// ]
        ///
        /// Ejemplo de respuesta 404:
        ///
        /// {
        ///   "errorCode": "NTF-003",
        ///   "errorMessage": "No se encontraron notificaciones para el usuario."
        /// }
        /// </remarks>
        /// <param name="userId">Identificador del usuario.</param>
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
