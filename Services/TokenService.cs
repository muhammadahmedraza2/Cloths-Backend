using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClothingErp.Api.Models;
using CLOTHS_ERP.API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ClothingErp.Api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(
        AppUser user,
        string role)
    {
        var section =
            _configuration.GetSection("JwtSettings");

        var secret =
            section["SecretKey"]
            ?? throw new InvalidOperationException(
                "JwtSettings:SecretKey is not configured.");

        var issuer =
            section["Issuer"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Issuer is not configured.");

        var audience =
            section["Audience"]
            ?? throw new InvalidOperationException(
                "JwtSettings:Audience is not configured.");

        var expiryMinutes =
            section.GetValue<int?>(
                "AccessTokenMinutes") ?? 60;

        var expiresAt =
            DateTime.UtcNow.AddMinutes(
                expiryMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.UserName ?? string.Empty),

            new(
                ClaimTypes.Role,
                role),

            /*
             * IMPORTANT:
             * Menu/API can read PC_ID directly.
             */
            new(
                "PC_ID",
                user.PcId.ToString()),

            /*
             * Keep PcId as well for compatibility
             * with any existing code.
             */
            new(
                "PcId",
                user.PcId.ToString()),

            new(
                "FullName",
                user.FullName ?? string.Empty)
        };

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));

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

        var tokenString =
            new JwtSecurityTokenHandler()
                .WriteToken(token);

        return (
            tokenString,
            expiresAt);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64));
    }

    public string HashRefreshToken(
        string refreshToken)
    {
        var bytes =
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    refreshToken));

        return Convert.ToHexString(bytes);
    }
}