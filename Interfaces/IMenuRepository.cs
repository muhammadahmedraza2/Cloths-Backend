using CLOTHS_ERP.API.Dtos;

namespace CLOTHS_ERP.API.Interfaces;

public interface IMenuRepository
{
    Task<List<MenuNodeDto>> GetMenuAsync(
        int pcId,
        CancellationToken cancellationToken = default);
}

