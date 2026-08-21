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

        private static List<RefreshToken> refreshTokens = new();

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

            var refreshToken = _jwtService.GenerateRefreshToken();

            refreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Username = request.Username,
                Role = user.Role,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new LoginResponse { AccessToken=token, RefreshToken= refreshToken });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh(RefreshTokenRequest request)
        {
            var storedToken = refreshTokens
                .FirstOrDefault(x => x.Token == request.RefreshToken);

            if (storedToken == null)
            {
                return Unauthorized("Invalid refresh token");
            }

            if (storedToken.IsRevoked)
            {
                return Unauthorized("Refresh token has already been used");
            }

            if (storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                return Unauthorized("Refresh token expired");
            }

            // Generate new access token
            var newAccessToken =
                _jwtService.GenerateToken(
                    storedToken.Username,
                    storedToken.Role);

            // Revoke OLD refresh token
            storedToken.IsRevoked = true;

            // Generate NEW refresh token
            var newRefreshToken =
                _jwtService.GenerateRefreshToken();

            // Store NEW refresh token
            refreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                Username = storedToken.Username,
                Role = storedToken.Role,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });

            return Ok(new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
    }
}
