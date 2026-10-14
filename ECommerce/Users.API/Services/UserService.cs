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

        public LoginResponseDto Login(LoginRequestDto request)
        {
            User? usuarioEncontrado = null;

            foreach (User u in _usuariosDb)
            {
                if (u.Email == request.Email)
                {
                    usuarioEncontrado = u;
                    break;
                }
            }

            if (usuarioEncontrado == null)
            {
                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
            }

            if (!usuarioEncontrado.Activo)
            {
                if (usuarioEncontrado.IntentosFallidos >= 3)
                {
                    throw new ForbiddenException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                }
                else
                {
                    throw new ForbiddenException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");
                }
            }

            if (usuarioEncontrado.PasswordHash != request.Password)
            {
                usuarioEncontrado.IntentosFallidos++;

                if (usuarioEncontrado.IntentosFallidos >= 3)
                {
                    usuarioEncontrado.Activo = false;
                }

                throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
            }

            usuarioEncontrado.IntentosFallidos = 0;

            LoginResponseDto response = new LoginResponseDto();
            response.Id = usuarioEncontrado.Id;
            response.Nombre = usuarioEncontrado.Nombre;
            response.Apellido = usuarioEncontrado.Apellido;
            response.Email = usuarioEncontrado.Email;

            return response;
        }
    }
}