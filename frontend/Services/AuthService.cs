// Client/Services/ApiClient.cs
using System.Net.Http.Json;
using Contracts.Auth;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Services;

public class AuthService
{
    
    private readonly HttpClient _http;
    public AuthService(HttpClient http) => _http = http;

    private static void IncludeCookies(HttpRequestMessage req) =>
        req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
   
    public Action<bool, Roles> onAuthStatusChanged;



    public async Task<(bool success, MeResponse content)> LoginAsync(string nameOrEmail, string password)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
        {
            Content = JsonContent.Create(new { usernameOrEmail = nameOrEmail, password })
        };
        IncludeCookies(req);
        var res = await _http.SendAsync(req);
        var content = await res.Content.ReadFromJsonAsync<MeResponse>();
        
        if(content == null)
        {
            throw new Exception("Login response was null!");
        }
        
        if (onAuthStatusChanged != null)
            onAuthStatusChanged(res.IsSuccessStatusCode, content.Roles);


        return (res.IsSuccessStatusCode, content);
    }

    public async Task<(bool success, string content)> LogoutAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        IncludeCookies(req);
        var res = await _http.SendAsync(req);
        var body = await res.Content.ReadAsStringAsync();
        
        

        if (onAuthStatusChanged != null)
            onAuthStatusChanged(!res.IsSuccessStatusCode, Roles.None);
        return (res.IsSuccessStatusCode, body);
    }

    public async Task<(bool success, MeResponse content)> RegisterAsync(string username, string email, string password)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/register")
        {
            Content = JsonContent.Create(new { username, email, password })
        };
        IncludeCookies(req);
        var res = await _http.SendAsync(req);
        var content = await res.Content.ReadFromJsonAsync<MeResponse>();

        if (content == null)
        {
            throw new Exception("Register response was null!");
        }

        if (onAuthStatusChanged != null)
            onAuthStatusChanged(res.IsSuccessStatusCode, content.Roles);
        return (res.IsSuccessStatusCode, content);
    }

    public async Task<(bool success, MeResponse content)> MeAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        IncludeCookies(req);
        var res = await _http.SendAsync(req);

        var body = await res.Content.ReadFromJsonAsync<MeResponse>();

        if (body == null)
        {
            throw new Exception("Me response was null!");
        }

        if (onAuthStatusChanged != null)
            onAuthStatusChanged(res.IsSuccessStatusCode, body.Roles);
        return (res.IsSuccessStatusCode, body);
    }
}
