using Contracts.Auth;

public class AuthState
{
    public bool Authed { get; private set; }
    public Roles Roles { get; private set; } = Roles.None;

    // Notify interested components to re-render
    public event Action? Changed;

    public void Set(bool authed, Roles roles)
    {
        Authed = authed;
        Roles = roles;
        Changed?.Invoke();
    }
}