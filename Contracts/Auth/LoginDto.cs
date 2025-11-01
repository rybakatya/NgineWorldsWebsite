using System.ComponentModel.DataAnnotations;

namespace Contracts.Auth
{
    public record LoginDto
    {
        [Required, EmailAddress] public string Email { get; init; } = "";
        [Required] public string Username { get; set; } = "";
        [Required] public string Password { get; init; } = "";
    }
}
