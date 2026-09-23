using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ClothingErp.Api.Interfaces;
using ClothingErp.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace ClothingErp.Api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string token, DateTime expiresAt)
        GenerateToken(AppUser user)
    {
        var jwtSection =
            _configuration.GetSection("JwtSettings");

        var secretKey =
            jwtSection["SecretKey"]
            ?? throw new InvalidOperationException(
                "JwtSettings:SecretKey is not configured.");

        var issuer =
            jwtSection["Issuer"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Issuer is not configured.");

        var audience =
            jwtSection["Audience"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Audience is not configured.");

        var expiryDays =
            jwtSection.GetValue<int>("ExpiryDays");

        var expiresAt =
            DateTime.UtcNow.AddDays(expiryDays);

        var claims =
            new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(
                    System.Security.Claims.ClaimTypes.Name,
                    user.Username),

                new System.Security.Claims.Claim(
                    "PcId",
                    user.PcId.ToString())
            };

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

        return
        (
            new JwtSecurityTokenHandler()
                .WriteToken(token),

            expiresAt
        );
    }
}