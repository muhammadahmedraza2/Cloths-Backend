using System.Security.Claims;
using CLOTHS_ERP.API.Interfaces;
using CLOTHS_ERP.API.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CLOTHS_ERP.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuService _menuService;
    private readonly MenuOptions _options;

    public MenuController(
        IMenuService menuService,
        IOptions<MenuOptions> options)
    {
        _menuService = menuService;
        _options = options.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetMenu(
        CancellationToken cancellationToken)
    {
        var pcIdValue = User.FindFirst(
            _options.PcIdClaimType)?.Value;

        if (string.IsNullOrWhiteSpace(pcIdValue))
        {
            return Unauthorized(new
            {
                message = "PcId claim is missing."
            });
        }

        if (!int.TryParse(pcIdValue, out var pcId) || pcId <= 0)
        {
            return Unauthorized(new
            {
                message = "Invalid PcId."
            });
        }

        var role =
            User.FindFirst(ClaimTypes.Role)?.Value
            ?? User.FindFirst("role")?.Value;

        if (string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized(new
            {
                message = "Role claim is missing."
            });
        }

        var menu = await _menuService.GetMenuAsync(
            pcId,
            role,
            cancellationToken);

        return Ok(new
        {
            pcId,
            role,
            menu
        });
    }
}