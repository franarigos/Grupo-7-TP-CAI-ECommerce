
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        /// Ejemplo de respuesta 409:
        ///
        /// {
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
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserRequest request)
        {
            RegisterUserResponse response =
                await _userService.RegisterAsync(request);

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
        /// Ejemplo de respuesta 401:
        ///
        /// {
        ///   "errorCode": "USR-003",
        ///   "errorMessage": "Credenciales incorrectas."
        /// }
        ///
        /// Ejemplo de respuesta 403:
        ///
        /// {
        ///   "errorCode": "USR-004",
        ///   "errorMessage": "Usuario bloqueado por demasiados intentos."
        /// }
        /// </remarks>
        /// <param name="request">Credenciales del usuario.</param>
        /// <response code="200">Autenticación exitosa.</response>
        /// <response code="400">USR-002: Datos inválidos.</response>
        /// <response code="401">USR-003: Credenciales incorrectas.</response>
        /// <response code="403">USR-004 o USR-005: Cuenta bloqueada.</response>
        /// <response code="500">USR-006: Error interno.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 401)]
        [ProducesResponseType(typeof(ApiErrorResponse), 403)]
        [ProducesResponseType(typeof(ApiErrorResponse), 500)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request)
        {
            LoginResponseDto response =
                await _userService.LoginAsync(request);

            return Ok(response);
        }

        /// <summary>
        /// Consulta un usuario por su identificador.
        /// </summary>
        /// <param name="id">Identificador del usuario.</param>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">Usuario no encontrado.</response>
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
