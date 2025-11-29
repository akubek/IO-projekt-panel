using Microsoft.AspNetCore.Mvc;



namespace IO_Panel.Server.Controllers
{
    // Record dla danych logowania przychodzących z Reacta
    public record LoginRequest(string Username, string Password);

    // Record dla odpowiedzi sukcesu (w przyszłości będzie tu JWT)
    public record LoginResponse(string Username, string Role);

    [ApiController]
    [Route("auth")] // Nowa ścieżka: /auth
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        [HttpPost("login")] // Endpoint: POST /auth/login
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Attempting login for user: {Username}", request.Username);

            // Symulacja sprawdzenia hasła w bazie danych
            if (request.Username == "admin" && request.Password == "admin")
            {
                // Zalogowany = Admin (zgodnie z założeniem)
                _logger.LogInformation("Login successful for Admin.");
                return Ok(new LoginResponse(request.Username, "Admin"));
            }

            _logger.LogWarning("Login failed for user: {Username}", request.Username);
            // Zwraca kod 401 (Unauthorized) w przypadku nieudanej próby
            return Unauthorized(new { message = "Invalid credentials" });
        }
    }
}
