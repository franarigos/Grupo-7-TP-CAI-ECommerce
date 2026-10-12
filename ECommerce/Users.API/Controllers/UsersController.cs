using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController()
        {
            _userService = new UserService();
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserRequest request)
        {
            RegisterUserResponse response = _userService.Register(request);
            return Created($"/api/users/{response.Id}", response);
        }
    }
}