using CLOTHS_ERP.API.Dtos;

namespace CLOTHS_ERP.API.Interfaces;

public interface IMenuService
{
    Task<List<MenuNodeDto>> GetMenuAsync(
        int pcId,
        string role,
        CancellationToken cancellationToken = default);
}