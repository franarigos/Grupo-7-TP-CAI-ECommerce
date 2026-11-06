using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs
{
    public class SendNotificationRequest
    {
        /// <summary>
        /// Identificador del usuario destinatario.
        /// </summary>
        public Guid UsuarioId { get; set; }

        /// <summary>
        /// Texto de la notificación.
        /// </summary>
        /// <example>Su orden fue confirmada.</example>
        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(500,
            ErrorMessage = "El mensaje no puede superar 500 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>
        /// Tipo permitido: Email, Push o SMS.
        /// </summary>
        /// <example>Email</example>
        [Required(ErrorMessage = "El tipo es obligatorio.")]
        [RegularExpression("^(Email|Push|SMS)$",
            ErrorMessage = "El tipo debe ser Email, Push o SMS.")]
        public string Tipo { get; set; } = string.Empty;
    }
}