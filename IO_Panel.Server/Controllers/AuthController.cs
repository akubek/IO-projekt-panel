using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using IO_Panel.Server.Models; // Added to use the AdminUser model

namespace IO_Panel.Server.Controllers
{
    // Record for incoming login data from React
    public record LoginRequest(string Username, string Password);

    // Record for successful response (JWT will be here in the future)
    public record LoginResponse(string Username, string Role);

    [ApiController]
    [Route("auth")] // New route: /auth
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        // Dependency Injection for secure password hashing (now using AdminUser)
        private readonly IPasswordHasher<AdminUser> _passwordHasher;

        // CONSTANT: Simulation of a hashed password from the database for the user "admin".
        private const string AdminHashedPassword = "AQAAAAIAAYagAAAAEN0YG8P22L0y5E3tLBKra0XOi + 8u07QP8PEE3bGWsU8VNidlq1Ki2Jin7pox/lBADA==";


        // Updated constructor signature
        public AuthController(ILogger<AuthController> logger, IPasswordHasher<AdminUser> passwordHasher)
        {
            _logger = logger;
            _passwordHasher = passwordHasher;
                     
        }

        [HttpPost("login")] // Endpoint: POST /auth/login
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Attempting login for user: {Username}", request.Username);
            

            // 1. Username check simulation (only "admin" is known)
            if (request.Username != "admin")
            {
                _logger.LogWarning("Login failed: User {Username} not found.", request.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // 2. SECURE PASSWORD VERIFICATION using PasswordHasher
            var verificationResult = _passwordHasher.VerifyHashedPassword(
                // We use a temporary AdminUser instance for verification
                new AdminUser { Username = request.Username },
                AdminHashedPassword, // Hash from the "database"
                request.Password // Password provided by the user
            );

            // 3. Evaluate verification result
            if (verificationResult == PasswordVerificationResult.Success ||
                verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                // Login successful = Admin 
                _logger.LogInformation("Login successful for Admin.");
                return Ok(new LoginResponse(request.Username, "Admin"));
            }

            // Login failed (Incorrect password)
            _logger.LogWarning("Login failed for user: {Username} (Incorrect password).", request.Username);
            // Returns 401 (Unauthorized) code on failure
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }
}