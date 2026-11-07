
using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs
{
    /// <summary>
    /// Datos para iniciar sesión.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>Correo electrónico del usuario.</summary>
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del email es inválido.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Contraseña del usuario.</summary>
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }
}
