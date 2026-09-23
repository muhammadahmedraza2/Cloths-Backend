using ClothingErp.Api.Models;

namespace ClothingErp.Api.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateToken(AppUser user);
}