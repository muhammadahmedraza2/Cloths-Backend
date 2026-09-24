using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ClothingErp.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace ClothingErp.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(AppUser user, string role);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration) => _configuration = configuration;

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(AppUser user, string role)
    {
        var section = _configuration.GetSection("JwtSettings");
        var secret = section["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
        var issuer = section["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured.");
        var audience = section["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience is not configured.");
        var expiryMinutes = section.GetValue<int?>("AccessTokenMinutes") ?? 60;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Role, role),
            new("PcId", user.PcId.ToString()),
            new("FullName", user.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expiresAt, signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}
