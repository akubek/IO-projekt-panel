using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IO_Panel.Server.Models;
using Microsoft.AspNetCore.Identity;
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
        private readonly IPasswordHasher<AdminUser> _passwordHasher;

        private const string AdminHashedPassword = "AQAAAAIAAYagAAAAEN0YG8P22L0y5E3tLBKra0XOi + 8u07QP8PEE3bGWsU8VNidlq1Ki2Jin7pox/lBADA==";
        
        private const string JwtKey = "dev-admin-auth-key-abcdefghijklmnoprstuwvxyz0123456789"; //32 byte key

        public AuthController(ILogger<AuthController> logger, IPasswordHasher<AdminUser> passwordHasher)
        {
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Attempting login for user: {Username}", request.Username);

            if (!string.Equals(request.Username, "admin", StringComparison.Ordinal))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(
                new AdminUser { Username = request.Username },
                AdminHashedPassword,
                request.Password);

            if (verificationResult != PasswordVerificationResult.Success &&
                verificationResult != PasswordVerificationResult.SuccessRehashNeeded)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
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