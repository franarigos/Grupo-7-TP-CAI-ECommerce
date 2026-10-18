using System;

namespace Users.API.Models
{
    /// <summary>
    /// Modelo de datos de User
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }

        /// <summary>
        /// Hash de la contraseña (nunca se expone en responses)
        /// </summary>
        public string PasswordHash { get; set; }

        public DateTime FechaRegistro { get; set; }

        /// <summary>
        /// false cuando el usuario está bloqueado
        /// </summary>
        public bool Activo { get; set; }

        /// <summary>
        /// Se incrementa en cada login fallido; se resetea al loguearse OK
        /// </summary>
        public int IntentosFallidos { get; set; }

        public User(string nombre, string apellido, string email, string passwordHash)
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