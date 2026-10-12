using System;

namespace Users.API.DTOs
{
    public class RegisterUserResponse
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
    }
}