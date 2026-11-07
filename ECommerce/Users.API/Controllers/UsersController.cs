
using System;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers
{
    /// <summary>
    /// API para registrar y autenticar usuarios.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Tags("Users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registra un nuevo usuario.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        /// {
        ///   "nombre": "Maria",
        ///   "apellido": "Gonzalez",
        ///   "email": "maria@example.com",
        ///   "password": "MiPassword123!"
        /// }
        ///
        /// Ejemplo de respuesta 201:
        ///
        /// {
        ///   "id": "a1b2c3d4-0000-0000-0000-111122223333",
        ///   "nombre": "Maria",
        ///   "apellido": "Gonzalez",
        ///   "email": "maria@example.com",
        ///   "fechaRegistro": "2026-11-07T10:00:00Z",
        ///   "activo": true
        /// }
        ///
        /// Ejemplo de respuesta 409:
        ///
        /// {
        ///   "status": 409,
        ///   "errorCode": "USR-001",
        ///   "errorMessage": "El email ya está registrado."
        /// }
        /// </remarks>
        /// <param name="request">Datos del usuario.</param>
        /// <response code="201">Usuario registrado.</response>
        /// <response code="400">USR-002: Datos inválidos.</response>
        /// <response code="409">USR-001: Email duplicado.</response>
        /// <response code="500">USR-006: Error interno.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterUserResponse), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public IActionResult Register(
            [FromBody] RegisterUserRequest request)
        {
            RegisterUserResponse response =
                _userService.Register(request);

            return StatusCode(201, response);
        }

        /// <summary>
        /// Autentica un usuario mediante email y contraseña.
        /// </summary>
        /// <remarks>
        /// Ejemplo de solicitud:
        ///
        /// {
        ///   "email": "maria@example.com",
        ///   "password": "MiPassword123!"
        /// }
        ///
        /// Ejemplo de respuesta 200:
        ///
        /// {
        ///   "id": "a1b2c3d4-0000-0000-0000-111122223333",
        ///   "nombre": "Maria",
        ///   "apellido": "Gonzalez",
        ///   "email": "maria@example.com"
        /// }
        ///
        /// Ejemplo de respuesta 401:
        ///
        /// {
        ///   "status": 401,
        ///   "errorCode": "USR-003",
        ///   "errorMessage": "Credenciales incorrectas."
        /// }
        ///
        /// Ejemplo de respuesta 403:
        ///
        /// {
        ///   "status": 403,
        ///   "errorCode": "USR-004",
        ///   "errorMessage": "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte."
        /// }
        /// </remarks>
        /// <param name="request">Credenciales del usuario.</param>
        /// <response code="200">Autenticación exitosa.</response>
        /// <response code="400">USR-002: Datos inválidos.</response>
        /// <response code="401">USR-003: Credenciales incorrectas.</response>
        /// <response code="403">USR-004: Demasiados intentos, o USR-005: Suspensión por fraude.</response>
        /// <response code="500">USR-006: Error interno.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 401)]
        [ProducesResponseType(typeof(ApiErrorResponse), 403)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public IActionResult Login(
            [FromBody] LoginRequestDto request)
        {
            LoginResponseDto response =
                _userService.Login(request);

            return Ok(response);
        }

        /// <summary>
        /// Consulta un usuario por su identificador.
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">USR-007: Usuario no encontrado.</response>
        /// <response code="500">USR-006: Error interno.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RegisterUserResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public IActionResult GetById(Guid id)
        {
            RegisterUserResponse? response =
                _userService.GetById(id);

            if (response == null)
            {
                HttpContext.Items["ErrorCode"] = "USR-007";

                Log.Warning(
                    "Usuario no encontrado. {errorCode}",
                    "USR-007");

                string correlationId =
                    HttpContext.Items["CorrelationId"]?.ToString()
                    ?? HttpContext.TraceIdentifier;

                ApiErrorResponse error = new ApiErrorResponse
                {
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Not Found",
                    Status = 404,
                    Detail = "El recurso solicitado no fue encontrado.",
                    Instance = HttpContext.Request.Path.ToString(),
                    ErrorCode = "USR-007",
                    ErrorMessage = "Usuario no encontrado.",
                    CorrelationId = correlationId
                };

                return NotFound(error);
            }

            return Ok(response);
        }
    }
}
