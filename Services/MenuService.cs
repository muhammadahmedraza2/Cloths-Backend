using CLOTHS_ERP.API.Dtos;
using CLOTHS_ERP.API.Interfaces;
using CLOTHS_ERP.API.Options;
using Microsoft.Extensions.Options;

namespace CLOTHS_ERP.API.Services;

public class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    private readonly MenuOptions _options;

    public MenuService(
        IMenuRepository menuRepository,
        IOptions<MenuOptions> options)
    {
        _menuRepository = menuRepository;
        _options = options.Value;
    }

    public async Task<List<MenuNodeDto>> GetMenuAsync(
        int pcId,
        string role,
        CancellationToken cancellationToken = default)
    {
        if (pcId <= 0)
        {
            throw new ArgumentException(
                "A valid PC_ID is required.",
                nameof(pcId));
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException(
                "A valid role is required.",
                nameof(role));
        }

        var normalizedRole = role.Trim();

        var isAdmin =
            normalizedRole.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase)
            ||
            normalizedRole.Equals(
                "Administrator",
                StringComparison.OrdinalIgnoreCase)
            ||
            normalizedRole.Equals(
                "System Administrator",
                StringComparison.OrdinalIgnoreCase);

        var isUser =
            normalizedRole.Equals(
                "User",
                StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !isUser)
        {
            return new List<MenuNodeDto>();
        }

        var effectivePcId = pcId;

        if (isAdmin && pcId == 0)
        {
            effectivePcId = _options.AdminPcId;
        }

        return await _menuRepository.GetMenuAsync(
            effectivePcId,
            cancellationToken);
    }
}
