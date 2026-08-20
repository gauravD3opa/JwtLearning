using JwtApi.Models;
using JwtApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var user = new User
            {
                Id = 1,
                Username = "gaurav",
                Password = "12345",
                Role = "Admin"
            };

            if (request.Username != user.Username ||
                request.Password != user.Password)
            {
                return Unauthorized("Invalid username or password");
            }

            var token = _jwtService.GenerateToken(user.Username,user.Role);

            return Ok(new LoginResponse { Token=token});
        }
    }
}
