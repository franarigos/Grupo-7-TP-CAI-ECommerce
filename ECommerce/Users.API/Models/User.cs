using System;

namespace Users.API.Models
{
    /// <summary>Representa un usuario.</summary>
    public class User
    {
        /// <summary>Identificador del usuario.</summary>
        public Guid Id { get; set; }

        /// <summary>Nombre del usuario.</summary>
        public string Nombre { get; set; }

        /// <summary>Apellido del usuario.</summary>
        public string Apellido { get; set; }

        /// <summary>Email del usuario.</summary>
        public string Email { get; set; }

        /// <summary>Hash de la contraseña; no se devuelve en las respuestas.</summary>
        public string PasswordHash { get; set; }

        /// <summary>Fecha UTC del registro.</summary>
        public DateTime FechaRegistro { get; set; }

        /// <summary>Indica si el usuario puede iniciar sesión.</summary>
        public bool Activo { get; set; }

        /// <summary>Cantidad de intentos fallidos consecutivos.</summary>
        public int IntentosFallidos { get; set; }

        /// <summary>Inicializa un nuevo usuario.</summary>
        public User(
            string nombre,
            string apellido,
            string email,
            string passwordHash)
        {
            Id = Guid.NewGuid();
            Nombre = nombre;
            Apellido = apellido;
            Email = email;
            PasswordHash = passwordHash;
            FechaRegistro = DateTime.UtcNow;
            Activo = true;
            IntentosFallidos = 0;
        }
    }
}