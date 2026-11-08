using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers
{
    /// <summary>Gestiona el registro y acceso de usuarios.</summary>
    [ApiController]
    [Route("api/users")]
    [Tags("Users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        /// <summary>Inicializa el controlador.</summary>
        /// <param name="userService">Servicio de usuarios.</param>
        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>Registra un usuario.</summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// {
        ///   "nombre": "Maria",
        ///   "apellido": "Gonzalez",
        ///   "email": "maria@example.com",
        ///   "password": "MiPassword123!"
        /// }
        /// </code>
        /// Respuesta 201:
        /// <code>
        /// {
        ///   "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "nombre": "Maria",
        ///   "apellido": "Gonzalez",
        ///   "email": "maria@example.com",
        ///   "fechaRegistro": "2026-11-07T12:00:00Z",
        ///   "activo": true
        /// }
        /// </code>
        /// Respuesta 409:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
        ///   "title": "Conflict",
        ///   "status": 409,
        ///   "detail": "Ya existe un recurso con esos datos o hay un conflicto de negocio.",
        ///   "instance": "/api/users/register",
        ///   "errorCode": "USR-001",
        ///   "errorMessage": "El email 'maria@example.com' ya está registrado.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">Datos del usuario.</param>
        /// <response code="201">Usuario registrado.</response>
        /// <response code="400">USR-002: Datos inválidos.</response>
        /// <response code="409">USR-001: Email duplicado.</response>
        /// <response code="415">USR-HTTP-415: Formato de contenido no admitido.</response>
        /// <response code="500">USR-006: Error interno.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterUserResponse), 201)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 409)]
        [ProducesResponseType(typeof(ApiErrorResponse), 415)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserRequest request)
        {
            RegisterUserResponse response =
                await _userService.RegisterAsync(request);

            return StatusCode(201, response);
        }

        /// <summary>Valida el email y la contraseña.</summary>
        /// <remarks>
        /// Solicitud:
        /// <code>
        /// {
        ///   "email": "maria@example.com",
        ///   "password": "MiPassword123!"
        /// }
        /// </code>
        /// Respuesta 200:
        /// <code>
        /// {
        ///   "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "nombre": "Maria",
        ///   "apellido": "Gonzalez",
        ///   "email": "maria@example.com"
        /// }
        /// </code>
        /// Respuesta 401:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
        ///   "title": "Unauthorized",
        ///   "status": 401,
        ///   "detail": "Las credenciales no son válidas.",
        ///   "instance": "/api/users/login",
        ///   "errorCode": "USR-003",
        ///   "errorMessage": "Credenciales incorrectas.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="request">Credenciales del usuario.</param>
        /// <response code="200">Credenciales correctas.</response>
        /// <response code="400">USR-002: Datos inválidos.</response>
        /// <response code="401">USR-003: Credenciales incorrectas.</response>
        /// <response code="403">USR-004: Bloqueo por tres intentos fallidos; USR-005: Suspensión manual.</response>
        /// <response code="415">USR-HTTP-415: Formato de contenido no admitido.</response>
        /// <response code="500">USR-006: Error interno.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 401)]
        [ProducesResponseType(typeof(ApiErrorResponse), 403)]
        [ProducesResponseType(typeof(ApiErrorResponse), 415)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request)
        {
            LoginResponseDto response =
                await _userService.LoginAsync(request);

            return Ok(response);
        }

        /// <summary>Consulta un usuario por ID.</summary>
        /// <remarks>
        /// Respuesta 200:
        /// <code>
        /// {
        ///   "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "nombre": "Maria",
        ///   "apellido": "Gonzalez",
        ///   "email": "maria@example.com",
        ///   "fechaRegistro": "2026-11-07T12:00:00Z",
        ///   "activo": true
        /// }
        /// </code>
        /// Respuesta 404:
        /// <code>
        /// {
        ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ///   "title": "Not Found",
        ///   "status": 404,
        ///   "detail": "El recurso solicitado no fue encontrado.",
        ///   "instance": "/api/users/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        ///   "errorCode": "USR-007",
        ///   "errorMessage": "Usuario no encontrado.",
        ///   "correlationId": "11111111-2222-3333-4444-555555555555"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="id">Identificador del usuario en formato GUID.</param>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">USR-007: Usuario no encontrado; extensión del catálogo para esta consulta.</response>
        /// <response code="500">USR-006: Error interno.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RegisterUserResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> GetById(Guid id)
        {
            RegisterUserResponse? response =
                await _userService.GetByIdAsync(id);

            if (response == null)
            {
                string correlationId =
                    HttpContext.Items["CorrelationId"]?.ToString()
                    ?? HttpContext.TraceIdentifier;

                HttpContext.Items["ErrorCode"] = "USR-007";

                ApiErrorResponse error = new ApiErrorResponse
                {
                    Type =
                        "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    Title = "Not Found",
                    Status = 404,
                    Detail =
                        "El recurso solicitado no fue encontrado.",
                    Instance =
                        HttpContext.Request.Path.ToString(),
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