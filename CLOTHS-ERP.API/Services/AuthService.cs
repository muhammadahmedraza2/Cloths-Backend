using ClothingErp.Api.Dtos;
using ClothingErp.Api.Interfaces;
using ClothingErp.Api.Models;
using CLOTHS_ERP.API.Dtos;

namespace ClothingErp.Api.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<AppUser> _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public AuthService(IRepository<AppUser> users, IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        var user = await _users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null || !_hasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        var (token, expiresAt) = _tokens.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }
}