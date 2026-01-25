using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace IO_Panel.Server.Controllers
{
    public record LoginRequest(string Username, string Password);

    public record LoginResponse(string Token, string Username, string Role);

    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Attempting login for user: {Username}", request.Username);

            var adminUsername = _configuration["AdminAuth:Username"] ?? "admin";
            var adminPassword = _configuration["AdminAuth:Password"] ?? "admin";
            var jwtKey = _configuration["AdminAuth:JwtKey"]
                ?? "dev-admin-auth-key-abcdefghijklmnoprstuwvxyz0123456789";

            if (!string.Equals(request.Username, adminUsername, StringComparison.Ordinal) ||
                !string.Equals(request.Password, adminPassword, StringComparison.Ordinal))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new LoginResponse(tokenString, request.Username, "Admin"));
        }
    }
}