using System;

namespace Users.API.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
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