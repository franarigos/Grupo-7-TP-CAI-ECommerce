
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Users.API.Data;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;
using ValidationException = Users.API.Exceptions.ValidationException;

namespace Users.API.Services
{
    public class UserService
    {
        private readonly UserRepository _repository;
        private readonly ILogger<UserService> _logger;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(
            UserRepository repository,
            ILogger<UserService> logger)
        {
            _repository = repository;
            _logger = logger;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<RegisterUserResponse> RegisterAsync(
            RegisterUserRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                !new EmailAddressAttribute().IsValid(request.Email))
            {
                _logger.LogWarning(
                    "Registro inválido {ErrorCode}",
                    "USR-002");

                throw new ValidationException(
                    "USR-002",
                    "Los datos del usuario son inválidos.");
            }

            string email = request.Email.Trim();

            User? existente =
                await _repository.GetByEmailAsync(email);

            if (existente != null)
            {
                _logger.LogWarning(
                    "Email duplicado {ErrorCode}",
                    "USR-001");

                throw new BusinessRuleException(
                    "USR-001",
                    $"El email '{email}' ya está registrado.");
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

            try
            {
                await _repository.CreateAsync(nuevoUsuario);
            }
            catch (SqliteException ex)
                when (ex.SqliteErrorCode == 19)
            {
                _logger.LogWarning(
                    "Email duplicado {ErrorCode}",
                    "USR-001");

                throw new BusinessRuleException(
                    "USR-001",
                    $"El email '{email}' ya está registrado.");
            }

            return MapearResponse(nuevoUsuario);
        }

        public async Task<LoginResponseDto> LoginAsync(
            LoginRequestDto request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                !new EmailAddressAttribute().IsValid(request.Email))
            {
                _logger.LogWarning(
                    "Login inválido {ErrorCode}",
                    "USR-002");

                throw new ValidationException(
                    "USR-002",
                    "Los datos del usuario son inválidos.");
            }

            User? usuario =
                await _repository.GetByEmailAsync(
                    request.Email.Trim());

            if (usuario == null)
            {
                _logger.LogWarning(
                    "Credenciales incorrectas {ErrorCode}",
                    "USR-003");

                throw new UnauthorizedException(
                    "USR-003",
                    "Credenciales incorrectas.");
            }

            if (!usuario.Activo)
            {
                if (usuario.IntentosFallidos >= 3)
                {
                    _logger.LogWarning(
                        "Usuario bloqueado {ErrorCode}",
                        "USR-004");

                    throw new ForbiddenException(
                        "USR-004",
                        "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                }

                _logger.LogWarning(
                    "Usuario suspendido {ErrorCode}",
                    "USR-005");

                throw new ForbiddenException(
                    "USR-005",
                    "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");
            }

            PasswordVerificationResult resultado =
                _passwordHasher.VerifyHashedPassword(
                    usuario,
                    usuario.PasswordHash,
                    request.Password);

            if (resultado == PasswordVerificationResult.Failed)
            {
                await _repository.AddFailedAttemptAsync(
                    usuario.Id);

                User? usuarioActualizado =
                    await _repository.GetByIdAsync(usuario.Id);

                if (usuarioActualizado != null &&
                    !usuarioActualizado.Activo &&
                    usuarioActualizado.IntentosFallidos >= 3)
                {
                    _logger.LogWarning(
                        "Usuario bloqueado {ErrorCode}",
                        "USR-004");

                    throw new ForbiddenException(
                        "USR-004",
                        "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");
                }

                _logger.LogWarning(
                    "Credenciales incorrectas {ErrorCode}",
                    "USR-003");

                throw new UnauthorizedException(
                    "USR-003",
                    "Credenciales incorrectas.");
            }

            await _repository.ResetFailedAttemptsAsync(
                usuario.Id);

            LoginResponseDto response = new LoginResponseDto();

            response.Id = usuario.Id;
            response.Nombre = usuario.Nombre;
            response.Apellido = usuario.Apellido;
            response.Email = usuario.Email;

            return response;
        }

        public async Task<RegisterUserResponse?> GetByIdAsync(
            Guid id)
        {
            User? usuario =
                await _repository.GetByIdAsync(id);

            if (usuario == null)
            {
                return null;
            }

            return MapearResponse(usuario);
        }

        private RegisterUserResponse MapearResponse(User usuario)
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
