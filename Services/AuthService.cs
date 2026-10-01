using System.Data;
using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using CLOTHS_ERP.API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext db,
        ITokenService tokens,
        IPasswordHasher<AppUser> passwordHasher,
        IConfiguration configuration)
    {
        _db = db;
        _tokens = tokens;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RegisterAsync(
        RegisterRequestDto dto)
    {
        try
        {
            var username = dto.Username?.Trim();

            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required.", null);

            if (string.IsNullOrWhiteSpace(dto.Password))
                return (false, "Password is required.", null);

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return (false, "Full name is required.", null);

            var existingUser = await GetUserByUsernameAsync(username);

            if (existingUser is not null)
                return (false, "Username already exists.", null);

            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                UserName = username,
                FullName = dto.FullName.Trim(),
                PcId = dto.PcId,
                Role = string.IsNullOrWhiteSpace(dto.Role)
                    ? "User"
                    : dto.Role.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Email = string.IsNullOrWhiteSpace(dto.Email)
                    ? null
                    : dto.Email.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber)
                    ? null
                    : dto.PhoneNumber.Trim()
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                dto.Password);

            await ExecuteInsertUserAsync(user);

            var response = await BuildLoginResponseAsync(user, user.Role);

            return (
                true,
                "User registered successfully.",
                response
            );
        }
        catch (SqlException ex)
        {
            return (
                false,
                ex.Message,
                null
            );
        }
        catch (Exception ex)
        {
            return (
                false,
                ex.Message,
                null
            );
        }
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> LoginAsync(
        LoginRequestDto dto)
    {
        try
        {
            var username = dto.Username?.Trim();

            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required.", null);

            if (string.IsNullOrWhiteSpace(dto.Password))
                return (false, "Password is required.", null);

            /*
             * USER GETTING IS HANDLED BY STORED PROCEDURE
             * dbo.sp_Users_GetByUsername
             */
            var user = await GetUserByUsernameAsync(username);

            if (user is null)
                return (false, "Invalid username or password.", null);

            if (!user.IsActive)
                return (false, "User account is inactive.", null);

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                return (false, "User password is not configured.", null);

            /*
             * Password verification is done against the PasswordHash
             * returned by the Stored Procedure.
             */
            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
                return (false, "Invalid username or password.", null);

            /*
             * Role is already returned by SP.
             */
            var role = string.IsNullOrWhiteSpace(user.Role)
                ? "User"
                : user.Role.Trim();

            if (role.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
      role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                role = "Admin";
            }
            else if (role.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                role = "User";
            }

            user.Role = role;

            var response = await BuildLoginResponseAsync(
                user,
                role);

            return (
                true,
                "Login successful.",
                response
            );
        }
        catch (SqlException ex)
        {
            return (
                false,
                ex.Message,
                null
            );
        }
        catch (Exception ex)
        {
            return (
                false,
                ex.Message,
                null
            );
        }
    }

    public async Task<(bool Success, string Message, LoginResponseDto? Data)> RefreshAsync(
        RefreshTokenRequestDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.RefreshToken))
                return (false, "Refresh token is required.", null);

            var hash = _tokens.HashRefreshToken(
                dto.RefreshToken);

            var token = await _db.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.TokenHash == hash);

            if (token is null)
                return (
                    false,
                    "Refresh token is invalid or expired.",
                    null
                );

            if (!token.IsActive)
                return (
                    false,
                    "Refresh token is invalid or expired.",
                    null
                );

            if (token.User is null)
                return (
                    false,
                    "User associated with refresh token was not found.",
                    null
                );

            if (!token.User.IsActive)
                return (
                    false,
                    "User account is inactive.",
                    null
                );

            /*
             * Current refresh token is revoked.
             */
            token.RevokedAt = DateTime.UtcNow;

            var role = string.IsNullOrWhiteSpace(token.User.Role)
                ? "User"
                : token.User.Role.Trim();

            if (role.Equals(
                    "Administrator",
                    StringComparison.OrdinalIgnoreCase))
            {
                role = "Admin";
            }

            token.User.Role = role;

            var response = await BuildLoginResponseAsync(
                token.User,
                role);

            await _db.SaveChangesAsync();

            return (
                true,
                "Token refreshed.",
                response
            );
        }
        catch (Exception ex)
        {
            return (
                false,
                ex.Message,
                null
            );
        }
    }

    public async Task LogoutAsync(Guid userId)
    {
        var activeTokens = await _db.RefreshTokens
            .Where(x =>
                x.UserId == userId &&
                x.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    /*
     * =========================================================
     * GET USER BY USERNAME
     * STORED PROCEDURE:
     * dbo.sp_Users_GetByUsername
     * =========================================================
     */
    private async Task<AppUser?> GetUserByUsernameAsync(
        string username)
    {
        await using var connection =
            new SqlConnection(
                _configuration.GetConnectionString(
                    "DefaultConnection"));

        await using var command =
            new SqlCommand(
                "dbo.sp_Users_GetByUsername",
                connection);

        command.CommandType =
            CommandType.StoredProcedure;

        command.Parameters.Add(
            new SqlParameter(
                "@Username",
                SqlDbType.NVarChar,
                100)
            {
                Value = username
            });

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var user = new AppUser
        {
            Id = reader.GetGuid(
                reader.GetOrdinal("Id")),

            UserName = reader["Username"]?.ToString() ?? string.Empty,

            PasswordHash = reader["PasswordHash"]?.ToString() ?? string.Empty,

            FullName = reader["FullName"]?.ToString() ?? string.Empty,

            Role = reader["Role"]?.ToString() ?? "User",

            PcId = reader["PcId"] == DBNull.Value
                ? 0
                : Convert.ToInt32(reader["PcId"]),

            CreatedAt = reader["CreatedAt"] == DBNull.Value
                ? DateTime.UtcNow
                : Convert.ToDateTime(reader["CreatedAt"]),

            IsActive = true
        };

        return user;
    }

    /*
     * =========================================================
     * INSERT USER
     * STORED PROCEDURE:
     * dbo.sp_Users_Insert
     * =========================================================
     */
    private async Task ExecuteInsertUserAsync(
        AppUser user)
    {
        await using var connection =
            new SqlConnection(
                _configuration.GetConnectionString(
                    "DefaultConnection"));

        await using var command =
            new SqlCommand(
                "dbo.sp_Users_Insert",
                connection);

        command.CommandType =
            CommandType.StoredProcedure;

        command.Parameters.Add(
            new SqlParameter(
                "@Id",
                SqlDbType.UniqueIdentifier)
            {
                Value = user.Id
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Username",
                SqlDbType.NVarChar,
                100)
            {
                Value = user.UserName ?? string.Empty
            });

        command.Parameters.Add(
            new SqlParameter(
                "@PasswordHash",
                SqlDbType.NVarChar,
                -1)
            {
                Value = user.PasswordHash ?? string.Empty
            });

        command.Parameters.Add(
            new SqlParameter(
                "@FullName",
                SqlDbType.NVarChar,
                200)
            {
                Value = user.FullName
            });

        command.Parameters.Add(
            new SqlParameter(
                "@Role",
                SqlDbType.NVarChar,
                50)
            {
                Value = user.Role
            });

        command.Parameters.Add(
            new SqlParameter(
                "@PcId",
                SqlDbType.Int)
            {
                Value = user.PcId
            });

        await connection.OpenAsync();

        await command.ExecuteNonQueryAsync();
    }

    /*
     * =========================================================
     * BUILD LOGIN RESPONSE
     * =========================================================
     */
    private async Task<LoginResponseDto> BuildLoginResponseAsync(
        AppUser user,
        string role)
    {
        var (accessToken, expiresAt) =
            _tokens.GenerateAccessToken(
                user,
                role);

        var refreshToken =
            _tokens.GenerateRefreshToken();

        var refreshDays =
            _configuration.GetValue<int?>(
                "JwtSettings:RefreshTokenDays") ?? 30;

        _db.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = user.Id,

                TokenHash =
                    _tokens.HashRefreshToken(
                        refreshToken),

                ExpiresAt =
                    DateTime.UtcNow.AddDays(
                        refreshDays)
            });

        await _db.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = accessToken,

            RefreshToken = refreshToken,

            Username = user.UserName ?? string.Empty,

            FullName = user.FullName,

            Role = role,

            UserId = user.Id,

            ExpiresAt = expiresAt
        };
    }
}