using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registrar nuevo usuario.
        /// </summary>
        /// <response code="201">Creado exitosamente.</response>
        /// <response code="400">USR-002</response>
        /// <response code="409">USR-001</response>
        /// <response code="500">USR-006</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterUserResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public IActionResult Register([FromBody] RegisterUserRequest request)
        {
            var response = _userService.Register(request);
            return StatusCode(201, response);
        }

        /// <summary>
        /// Autenticar usuario (email + password).
        /// </summary>
        /// <response code="200">Autenticación exitosa.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="401">USR-003</response>
        /// <response code="403">USR-004 o USR-005</response>
        /// <response code="500">USR-006</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            var response = _userService.Login(request);
            return Ok(response);
        }
        /// <summary>
        /// Consulta un usuario por su identificador.
        /// </summary>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">USR-007: usuario no encontrado.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RegisterUserResponse), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetById(Guid id)
        {
            RegisterUserResponse? response = _userService.GetById(id);

            if (response == null)
            {
                return NotFound(new
                {
                    type = "about:blank",
                    title = "Not Found",
                    status = 404,
                    detail = "Usuario no encontrado.",
                    instance = HttpContext.Request.Path.ToString(),
                    errorCode = "USR-007",
                    errorMessage = "Usuario no encontrado.",
                    correlationId =
                        HttpContext.Response.Headers["X-Correlation-Id"].ToString()
                });
            }

            return Ok(response);
        }
    }
}