using System.Security.Claims;
using BCrypt.Net;
using Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteAPI.Data;

using SiteAPI.Models;
using SiteAPI.Services;

namespace SiteAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, TokenService tokens, IConfiguration config)
    {
        _db = db;
        _tokens = tokens;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<ActionResult<MeResponse>> Register(RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) ||
            string.IsNullOrWhiteSpace(req.Email) ||
            string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("All fields are required.");

        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict("Username already taken.");

        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict("Email already in use.");

        var user = new User
        {
            Username = req.Username.Trim(),
            Email = req.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Roles = Roles.Member
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Optional: auto-login after register
        await IssueCookieAsync(user);
        return new MeResponse { Id = user.Id, Username = user.Username, Email = user.Email, Roles = user.Roles };
    }



    [HttpPost("login")]
    public async Task<ActionResult<MeResponse>> Login(LoginRequest req)
    {
        var name = req.UsernameOrEmail.Trim();
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == name || u.Email == name);

        if (user is null) return Unauthorized("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        await IssueCookieAsync(user);
        return new MeResponse { Id = user.Id, Username = user.Username, Email = user.Email, Roles = user.Roles };
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(ClaimTypes.Name)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        var userIdStr = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var user = await _db.Users.FindAsync(userId);
        if (user is null) return Unauthorized();

        return new MeResponse { Id = user.Id, Username = user.Username, Email = user.Email, Roles = user.Roles };
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var cookieName = _config["Jwt:CookieName"]!;
        Response.Cookies.Delete(cookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });
        return Ok(new { message = "Logged out" });
    }

    private Task IssueCookieAsync(User user)
    {
        var token = _tokens.CreateToken(user.Id, user.Username, user.Email, user.Roles);
        var cookieName = _config["Jwt:CookieName"]!;

        Response.Cookies.Append(cookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,            // true for HTTPS; for local HTTP dev you can set false if needed
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Task.CompletedTask;
    }
}
