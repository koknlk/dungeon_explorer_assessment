using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DungeonExplorerBackend.Contracts;
using DungeonExplorerBackend.Models.AuthLayer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DungeonExplorerBackend.Services;

public class JwtService : IJwtService
    {
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService()
        {
        _secret = Environment.GetEnvironmentVariable("JWT__Secret");

        if (_secret.Length < 32)
            throw new Exception("JWT Secret must be at least 32 characters");

        _issuer = Environment.GetEnvironmentVariable("JWT__Issuer");
        _audience = Environment.GetEnvironmentVariable("JWT__Audience");
        }

    public string GenerateToken(AppUser user)
        {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }