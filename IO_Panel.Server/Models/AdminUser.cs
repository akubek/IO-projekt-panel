namespace IO_Panel.Server.Models
{
    // User model for the Administrator role, required by PasswordHasher.
    public class AdminUser
    {
        public string Username { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
