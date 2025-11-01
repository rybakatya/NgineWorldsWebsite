using System.ComponentModel.DataAnnotations;

namespace Contracts.Auth
{
    public record RegisterDto
    {
        [Required, EmailAddress] public string Email { get; init; } = "";
        [Required, MinLength(3)] public string Username { get; set; } = "";
        [Required, MinLength(6)] public string Password { get; init; } = "";
        public string? Role { get; init; }
    }
}
