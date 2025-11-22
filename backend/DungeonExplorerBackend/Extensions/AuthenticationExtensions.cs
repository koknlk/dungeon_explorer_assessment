using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DungeonExplorerBackend.Extensions;

public static class AuthenticationExtensions
    {
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
        {
        var secret = Environment.GetEnvironmentVariable("JWT__Secret");
        var issuer = Environment.GetEnvironmentVariable("JWT__Issuer");
        var audience = Environment.GetEnvironmentVariable("JWT__Audience");

        var key = Encoding.UTF8.GetBytes(secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
                {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
                };
        });

        return services;
        }
    }