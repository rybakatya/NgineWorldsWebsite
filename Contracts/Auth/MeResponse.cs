using Contracts.Auth;

public class MeResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public Roles Roles { get; set; } = default;
}