using ClothingErp.Api.Models;

namespace CLOTHS_ERP.API.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(
            AppUser user,
            string role);

        string GenerateRefreshToken();

        string HashRefreshToken(
            string refreshToken);
    }




}
