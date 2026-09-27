namespace ClothingErp.Api.Dtos;

public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public int PcId { get; set; }

    public string Role { get; set; } = "User";
}


// ============================================================
// ADMIN CREATE USER
// ============================================================

public class AdminCreateUserRequestDto : RegisterRequestDto
{
}


// ============================================================
// LOGIN
// ============================================================

public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}


// ============================================================
// REFRESH TOKEN
// ============================================================

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}


// ============================================================
// LOGIN RESPONSE
// ============================================================

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public DateTime ExpiresAt { get; set; }
}


// ============================================================
// USER RESPONSE
// ============================================================

public class UserResponseDto
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = "User";
}


// ============================================================
// ROLE RESPONSE
// ============================================================

public class RoleResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}