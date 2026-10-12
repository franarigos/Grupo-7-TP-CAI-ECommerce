using System.Collections.Generic;
using Users.API.Models;
using Users.API.DTOs;
using Users.API.Exceptions;

namespace Users.API.Services
{
    public class UserService
    {
        private static List<User> _usuariosDb = new List<User>();

        public RegisterUserResponse Register(RegisterUserRequest request)
        {
            if (string.IsNullOrEmpty(request.Nombre) ||
                string.IsNullOrEmpty(request.Apellido) ||
                string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password))
            {
                throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");
            }

            bool emailExiste = false;
            foreach (User usuarioExistente in _usuariosDb)
            {
                if (usuarioExistente.Email == request.Email)
                {
                    emailExiste = true;
                }
            }

            if (emailExiste)
            {
                throw new BusinessRuleException("USR-001", $"El email '{request.Email}' ya está registrado.");
            }

            User nuevoUsuario = new User(request.Nombre, request.Apellido, request.Email, request.Password);

            _usuariosDb.Add(nuevoUsuario);

            RegisterUserResponse response = new RegisterUserResponse();
            response.Id = nuevoUsuario.Id;
            response.Nombre = nuevoUsuario.Nombre;
            response.Apellido = nuevoUsuario.Apellido;
            response.Email = nuevoUsuario.Email;
            response.FechaRegistro = nuevoUsuario.FechaRegistro;
            response.Activo = nuevoUsuario.Activo;

            return response;
        }
    }
}