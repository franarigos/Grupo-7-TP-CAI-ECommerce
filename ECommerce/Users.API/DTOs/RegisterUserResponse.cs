
using System;

namespace Users.API.DTOs
{
    /// <summary>
    /// Datos devueltos al registrar un usuario.
    /// </summary>
    public class RegisterUserResponse
    {
        /// <summary>Identificador del usuario.</summary>
        public Guid Id { get; set; }

        /// <summary>Nombre del usuario.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Apellido del usuario.</summary>
        public string Apellido { get; set; } = string.Empty;

        /// <summary>Correo electrónico del usuario.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Fecha de registro.</summary>
        public DateTime FechaRegistro { get; set; }

        /// <summary>Estado de la cuenta.</summary>
        public bool Activo { get; set; }
    }
}
