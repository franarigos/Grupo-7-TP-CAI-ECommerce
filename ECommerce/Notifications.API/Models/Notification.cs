
using System;

namespace Notifications.API.Models
{
    /// <summary>
    /// Notificación registrada para un usuario.
    /// </summary>
    public class Notification
    {
        /// <summary>Identificador de la notificación.</summary>
        public Guid Id { get; set; }

        /// <summary>Identificador del usuario destinatario.</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Mensaje de la notificación.</summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>Tipo de notificación.</summary>
        public string Tipo { get; set; } = string.Empty;

        /// <summary>Estado de la notificación.</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Fecha de envío simulado.</summary>
        public DateTime FechaEnvio { get; set; }
    }
}
