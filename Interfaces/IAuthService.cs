using ClothingErp.Api.Dtos;

namespace CLOTHS_ERP.API.Interfaces;

public interface IAuthService
{
    Task<(
        bool Success,
        string Message,
        LoginResponseDto? Data)> RegisterAsync(
            RegisterRequestDto dto);

    Task<(
        bool Success,
        string Message,
        LoginResponseDto? Data)> LoginAsync(
            LoginRequestDto dto);

    Task<(
        bool Success,
        string Message,
        LoginResponseDto? Data)> RefreshAsync(
            RefreshTokenRequestDto dto);

    Task LogoutAsync(Guid userId);
}