using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using Contracts.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace frontend.Services;

public sealed class AuthService
{
    private const string StorageKey = "auth_token";

    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigation;

    private AuthState _state = AuthState.SignedOut;

    public AuthService(HttpClient http, IJSRuntime js, NavigationManager navigation)
    {
        _http = http;
        _js = js;
        _navigation = navigation;
    }

    public event Action? AuthenticationStateChanged;

    public AuthState State => _state;

    public bool IsAuthenticated => _state.IsAuthenticated;

    public async Task InitializeAsync()
    {
        var stored = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (string.IsNullOrWhiteSpace(stored))
        {
            return;
        }

        try
        {
            var response = JsonSerializer.Deserialize<AuthResponse>(stored);
            if (response is null)
            {
                await ClearStoredAsync();
                return;
            }

            if (!TryCreateState(response, out var state))
            {
                await ClearStoredAsync();
                return;
            }

            _state = state;
            AuthenticationStateChanged?.Invoke();
        }
        catch
        {
            await ClearStoredAsync();
        }
    }

    public async Task SignInAsync(AuthResponse response)
    {
        if (!TryCreateState(response, out var state))
        {
            throw new InvalidOperationException("Unable to parse authentication token.");
        }

        var json = JsonSerializer.Serialize(response);
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);

        _state = state;
        AuthenticationStateChanged?.Invoke();
    }

    public async Task SignOutAsync()
    {
        try
        {
            // Invalidate the server-side auth cookie if possible.
            var response = await _http.PostAsync("api/auth/logout", null);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Best-effort; even if the request fails we still clear client state.
        }

        await ClearStoredAsync();
        _state = AuthState.SignedOut;
        AuthenticationStateChanged?.Invoke();

        _navigation.NavigateTo("/");
    }

    private async Task ClearStoredAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }

    private static bool TryCreateState(AuthResponse response, out AuthState state)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var jwt = handler.ReadJwtToken(response.AccessToken);
            var expires = jwt.ValidTo == DateTime.MinValue
                ? (DateTimeOffset?)null
                : new DateTimeOffset(DateTime.SpecifyKind(jwt.ValidTo, DateTimeKind.Utc));

            if (expires is { } exp && exp <= DateTimeOffset.UtcNow)
            {
                state = AuthState.SignedOut;
                return false;
            }

            var nameClaim = jwt.Claims.FirstOrDefault(c =>
                c.Type is "name" or "unique_name" or "preferred_username" or "email" or JwtRegisteredClaimNames.Sub);

            state = new AuthState(true, response, expires, nameClaim?.Value);
            return true;
        }
        catch
        {
            state = AuthState.SignedOut;
            return false;
        }
    }
}

public record AuthState(bool IsAuthenticated, AuthResponse? Response, DateTimeOffset? ExpiresAt, string? DisplayName)
{
    public static AuthState SignedOut { get; } = new(false, null, null, null);
}
