namespace IO_Panel.Server.Models
{
    /// <summary>
    /// Minimal user representation used for admin authentication utilities (e.g., <c>PasswordHasher&lt;TUser&gt;</c>).
    /// This project authenticates a single configured admin user and uses JWT role claims for authorization.
    /// </summary>
    public class AdminUser
    {
        public string Username { get; set; } = default!;
        public string Role { get; set; } = default!;
    }
}
