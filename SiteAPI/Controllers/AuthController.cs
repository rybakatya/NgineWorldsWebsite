using AuthApi.Data;
using AuthApi.Services;
using Contracts.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;
    private readonly IJwtTokenService _jwt;

    public AuthController(UserManager<ApplicationUser> users,
                          RoleManager<IdentityRole> roles,
                          IJwtTokenService jwt)
    {
        _users = users;
        _roles = roles;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new ApplicationUser { UserName = dto.Username, Email = dto.Email, EmailConfirmed = true };
        var result = await _users.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!await _roles.RoleExistsAsync(dto.Role))
                await _roles.CreateAsync(new IdentityRole(dto.Role));
            await _users.AddToRoleAsync(user, dto.Role);
        }

        var token = await _jwt.CreateTokenAsync(user);
        var roles = (await _users.GetRolesAsync(user)).ToArray();

        SetAuthCookie(token);
        return Ok(new AuthResponse(token, "Bearer", roles));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null) return Unauthorized();

        var valid = await _users.CheckPasswordAsync(user, dto.Password);
        if (!valid) return Unauthorized();

        var token = await _jwt.CreateTokenAsync(user);
        var roles = (await _users.GetRolesAsync(user)).ToArray();
        return Ok(new AuthResponse(token, "Bearer", roles));
    }

    private void SetAuthCookie(string cookie)
    {

    }
}
