using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Tags("Notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _service;

        public NotificationsController(NotificationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Registra y simula el envío de una notificación.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        ///     {
        ///       "usuarioId": "11111111-1111-1111-1111-111111111111",
        ///       "mensaje": "Su orden fue confirmada.",
        ///       "tipo": "Email"
        ///     }
        ///
        /// El usuario debe existir en Users.API.
        /// No se envían correos ni SMS reales.
        ///
        /// Ejemplo de respuesta de error:
        ///
        ///     {
        ///       "type": "about:blank",
        ///       "title": "Not Found",
        ///       "status": 404,
        ///       "detail": "Usuario no encontrado.",
        ///       "instance": "/api/notifications/send",
        ///       "errorCode": "NTF-001",
        ///       "errorMessage": "Usuario no encontrado.",
        ///       "correlationId": "22222222-2222-2222-2222-222222222222"
        ///     }
        /// </remarks>
        /// <response code="201">Notificación registrada con estado Enviada.</response>
        /// <response code="400">NTF-002: datos inválidos.</response>
        /// <response code="404">NTF-001: usuario no encontrado.</response>
        /// <response code="500">NTF-004: error interno o fallo de dependencia.</response>
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
                await _service.SendAsync(request, cancellationToken);

            return StatusCode(201, response);
        }

        /// <summary>
        /// Lista las notificaciones registradas para un usuario.
        /// </summary>
        /// <param name="userId">Identificador del usuario.</param>
        /// <remarks>
        /// Si no hay notificaciones se devuelve HTTP 404, NTF-003.
        ///
        /// Ejemplo de respuesta de error:
        ///
        ///     {
        ///       "type": "about:blank",
        ///       "title": "Not Found",
        ///       "status": 404,
        ///       "detail": "No se encontraron notificaciones para el usuario.",
        ///       "instance": "/api/notifications/11111111-1111-1111-1111-111111111111",
        ///       "errorCode": "NTF-003",
        ///       "errorMessage": "No se encontraron notificaciones para el usuario.",
        ///       "correlationId": "22222222-2222-2222-2222-222222222222"
        ///     }
        /// </remarks>
        /// <response code="200">Lista de notificaciones.</response>
        /// <response code="400">NTF-002: identificador inválido.</response>
        /// <response code="404">NTF-003: no hay notificaciones.</response>
        /// <response code="500">NTF-004: error interno.</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(List<NotificationResponse>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public IActionResult GetByUser(Guid userId)
        {
            List<NotificationResponse> response =
                _service.GetByUser(userId);

            return Ok(response);
        }
    }
}
