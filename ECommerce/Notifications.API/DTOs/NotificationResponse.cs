namespace Notifications.API.DTOs
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }

        public Guid UsuarioId { get; set; }

        /// <example>Su orden fue confirmada.</example>
        public string Mensaje { get; set; } = string.Empty;

        /// <example>Email</example>
        public string Tipo { get; set; } = string.Empty;

        /// <example>Enviada</example>
        public string Estado { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; }
    }
}
