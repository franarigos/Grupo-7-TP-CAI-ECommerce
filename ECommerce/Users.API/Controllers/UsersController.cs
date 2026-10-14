using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;
using Users.API.Models; 

namespace Users.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
        public IActionResult Register([FromBody] RegisterUserRequest request)
        {
            RegisterUserResponse response = _userService.Register(request);
            return Created($"api/users/{response.Id}", response);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public IActionResult Login([FromBody] LoginRequestDto request)
        {
            var response = _userService.Login(request);
            return Ok(response);
        }
    }
}