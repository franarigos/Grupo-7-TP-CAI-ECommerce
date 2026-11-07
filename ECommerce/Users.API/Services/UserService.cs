
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;

namespace Users.API.Services
{
    public class UserService
    {
        private static readonly List<User> _usuariosDb =
            new List<User>();

        private static readonly object _candado = new object();

        private readonly PasswordHasher<User> _passwordHasher =
            new PasswordHasher<User>();

        public RegisterUserResponse Register(
            RegisterUserRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                !new EmailAddressAttribute().IsValid(request.Email))
            {
                Log.Warning(
                    "Registro inválido. {errorCode}",
                    "USR-002");

                throw new ValidationException(
                    "USR-002",
                    "Los datos del usuario son inválidos.");
            }

            string email = request.Email.Trim();

            lock (_candado)
            {
                foreach (User usuario in _usuariosDb)
                {
                    if (string.Equals(
                        usuario.Email,
                        email,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Warning(
                            "Email duplicado. {errorCode}",
                            "USR-001");

                        throw new BusinessRuleException(
                            "USR-001",
                            $"El email '{email}' ya está registrado.");
                    }
                }

                User nuevoUsuario = new User(
                    request.Nombre.Trim(),
                    request.Apellido.Trim(),
                    email,
                    string.Empty);

                nuevoUsuario.PasswordHash =
                    _passwordHasher.HashPassword(
                        nuevoUsuario,
                        request.Password);

                _usuariosDb.Add(nuevoUsuario);

                return MapearUsuario(nuevoUsuario);
            }
        }

        public LoginResponseDto Login(
            LoginRequestDto request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                !new EmailAddressAttribute().IsValid(request.Email))
            {
                Log.Warning(
                    "Login inválido. {errorCode}",
                    "USR-002");

                throw new ValidationException(
                    "USR-002",
                    "Los datos del usuario son inválidos.");
            }

            string email = request.Email.Trim();

            lock (_candado)
            {
                User? usuarioEncontrado = null;

                foreach (User usuario in _usuariosDb)
                {
                    if (string.Equals(
                        usuario.Email,
                        email,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        usuarioEncontrado = usuario;
                        break;
                    }
                }

                if (usuarioEncontrado == null)
                {
                    Log.Warning(
                        "Credenciales incorrectas. {errorCode}",
                        "USR-003");

                    throw new UnauthorizedException(
                        "USR-003",
                        "Credenciales incorrectas.");
                }

                if (!usuarioEncontrado.Activo)
                {
                    if (usuarioEncontrado.IntentosFallidos >= 3)
                    {
                        Log.Warning(
                            "Usuario bloqueado. {errorCode}",
                            "USR-004");

                        throw new ForbiddenException(
                            "USR-004",
                            "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                    }

                    Log.Warning(
                        "Usuario suspendido. {errorCode}",
                        "USR-005");

                    throw new ForbiddenException(
                        "USR-005",
                        "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");
                }

                PasswordVerificationResult resultado =
                    _passwordHasher.VerifyHashedPassword(
                        usuarioEncontrado,
                        usuarioEncontrado.PasswordHash,
                        request.Password);

                if (resultado == PasswordVerificationResult.Failed)
                {
                    usuarioEncontrado.IntentosFallidos++;

                    if (usuarioEncontrado.IntentosFallidos >= 3)
                    {
                        usuarioEncontrado.Activo = false;

                        Log.Warning(
                            "Usuario bloqueado. {errorCode}",
                            "USR-004");

                        throw new ForbiddenException(
                            "USR-004",
                            "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                    }

                    Log.Warning(
                        "Credenciales incorrectas. {errorCode}",
                        "USR-003");

                    throw new UnauthorizedException(
                        "USR-003",
                        "Credenciales incorrectas.");
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

        public RegisterUserResponse? GetById(Guid id)
        {
            lock (_candado)
            {
                foreach (User usuario in _usuariosDb)
                {
                    if (usuario.Id == id)
                    {
                        return MapearUsuario(usuario);
                    }
                }
            }

            return null;
        }

        private RegisterUserResponse MapearUsuario(User usuario)
        {
            RegisterUserResponse response =
                new RegisterUserResponse();

            response.Id = usuario.Id;
            response.Nombre = usuario.Nombre;
            response.Apellido = usuario.Apellido;
            response.Email = usuario.Email;
            response.FechaRegistro = usuario.FechaRegistro;
            response.Activo = usuario.Activo;

            return response;
        }
    }
}
