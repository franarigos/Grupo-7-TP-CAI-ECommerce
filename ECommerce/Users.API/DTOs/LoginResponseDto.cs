
using System;

namespace Users.API.DTOs
{
    /// <summary>
    /// Datos devueltos después de iniciar sesión.
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>Identificador del usuario.</summary>
        public Guid Id { get; set; }

        /// <summary>Nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellido del usuario.</summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>Correo electrónico del usuario.</summary>
        public string Email { get; set; } = string.Empty;
    }
}
