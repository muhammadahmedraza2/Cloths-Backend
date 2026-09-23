using ClothingErp.Api.Dtos;
using CLOTHS_ERP.API.Dtos;

namespace ClothingErp.Api.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string username, string password);
}