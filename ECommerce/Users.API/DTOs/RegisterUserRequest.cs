
using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    /// <summary>
    /// Datos para registrar un usuario.
    /// </summary>
    public class RegisterUserRequest
    {
        /// <summary>Nombre del usuario.</summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellido del usuario.</summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        public string Apellido { get; set; } = string.Empty;

        /// <summary>Correo electrónico del usuario.</summary>
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del email es inválido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Contraseña del usuario.</summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }
}
