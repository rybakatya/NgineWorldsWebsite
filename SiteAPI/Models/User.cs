using Contracts.Auth;

namespace SiteAPI.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public Roles Roles { get; set; } = Roles.User;  // default
}
