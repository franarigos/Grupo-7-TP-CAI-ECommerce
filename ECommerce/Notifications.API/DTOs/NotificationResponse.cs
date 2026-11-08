using System;

namespace Notifications.API.DTOs
{
    /// <summary>
    /// Datos de una notificación registrada.
    /// </summary>
    public class NotificationResponse
    {
        /// <summary>
        /// Identificador de la notificación.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador del usuario destinatario.
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Texto de la notificación.
        /// </summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de notificación: Email, Push o SMS.
        /// </summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>
        /// Estado del envío simulado.
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora UTC del envío.
        /// </summary>
        public DateTime FechaEnvio { get; set; }
    }
}