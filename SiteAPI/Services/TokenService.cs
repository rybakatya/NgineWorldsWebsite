// Services/TokenService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Contracts.Auth;
using Microsoft.IdentityModel.Tokens;
using SiteAPI.Models; // <-- for Roles enum

namespace SiteAPI.Services;

public class TokenService
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public TokenService(IConfiguration config)
    {
        _key = config["Jwt:Key"]!;
        _issuer = config["Jwt:Issuer"]!;
        _audience = config["Jwt:Audience"]!;
    }

    // NOTE: add Roles roles
    public string CreateToken(int userId, string username, string email, Roles roles)
    {
        var claims = new List<Claim>
        {
            // JWT standard claims
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Email, email),

            // Helpful for ASP.NET auth plumbing (optional but nice)
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
        };

        // Expand flags -> one ClaimTypes.Role per role
        void AddIf(Roles flag, string name)
        {
            if ((roles & flag) != 0)
                claims.Add(new Claim(ClaimTypes.Role, name));
        }

        AddIf(Roles.User, "User");
        AddIf(Roles.Member, "Member");
        AddIf(Roles.Moderator, "Moderator");
        AddIf(Roles.Admin, "Admin");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
