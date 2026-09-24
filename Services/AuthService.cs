using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(RegisterRequestDto dto);
    Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginRequestDto dto);
    Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshAsync(RefreshTokenRequestDto dto);
    Task LogoutAsync(Guid userId);
}

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;

    public AuthService(UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, AppDbContext db, ITokenService tokens)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
        _tokens = tokens;
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(RegisterRequestDto dto)
    {
        var username = dto.Username.Trim();
        if (await _userManager.FindByNameAsync(username) is not null)
            return (false, "Username already exists.", null);

        if (!string.IsNullOrWhiteSpace(dto.Email) && await _userManager.FindByEmailAsync(dto.Email.Trim()) is not null)
            return (false, "Email already exists.", null);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            FullName = dto.FullName.Trim(),
            PcId = dto.PcId,
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return (false, string.Join(" ", result.Errors.Select(x => x.Description)), null);

        await _userManager.AddToRoleAsync(user, "User");
        return (true, "User registered successfully.", await BuildLoginResponseAsync(user, "User"));
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username.Trim());
        if (user is null || !user.IsActive)
            return (false, "Invalid username or password.", null);

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            return (false, "Invalid username or password.", null);

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? (user.Role.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ? "Admin" : user.Role);
        if (role is "Administrator") role = "Admin";

        user.Role = role;
        await _userManager.UpdateAsync(user);

        return (true, "Login successful.", await BuildLoginResponseAsync(user, role));
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshAsync(RefreshTokenRequestDto dto)
    {
        var hash = _tokens.HashRefreshToken(dto.RefreshToken);
        var token = await _db.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.TokenHash == hash);

        if (token is null || !token.IsActive || token.User is null || !token.User.IsActive)
            return (false, "Refresh token is invalid or expired.", null);

        token.RevokedAt = DateTime.UtcNow;
        var roles = await _userManager.GetRolesAsync(token.User);
        var role = roles.FirstOrDefault() ?? token.User.Role;
        if (role is "Administrator") role = "Admin";

        return (true, "Token refreshed.", await BuildLoginResponseAsync(token.User, role));
    }

    public async Task LogoutAsync(Guid userId)
    {
        var active = await _db.RefreshTokens.Where(x => x.UserId == userId && x.RevokedAt == null).ToListAsync();
        foreach (var item in active) item.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task<LoginResponseDto> BuildLoginResponseAsync(AppUser user, string role)
    {
        var (access, expires) = _tokens.GenerateAccessToken(user, role);
        var refresh = _tokens.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokens.HashRefreshToken(refresh),
            ExpiresAt = DateTime.UtcNow.AddDays(_configurationRefreshDays())
        });
        await _db.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = access,
            RefreshToken = refresh,
            Username = user.UserName ?? "",
            FullName = user.FullName,
            Role = role,
            UserId = user.Id,
            ExpiresAt = expires
        };
    }

    private int _configurationRefreshDays()
    {
        // Keep refresh lifetime configurable without exposing secrets.
        return 30;
    }
}
