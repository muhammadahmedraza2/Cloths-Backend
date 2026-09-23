using CLOTHS_ERP.API.Dtos;
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
    public async Task<ActionResult<List<MenuNodeDto>>> GetMenu(
        CancellationToken cancellationToken)
    {
        var pcIdValue =
            User.FindFirst(
                _options.PcIdClaimType)?.Value;

        if (string.IsNullOrWhiteSpace(pcIdValue))
        {
            return Unauthorized(new
            {
                message =
                    "PcId claim is missing from JWT token."
            });
        }

        if (!int.TryParse(
                pcIdValue,
                out var pcId))
        {
            return Unauthorized(new
            {
                message =
                    "Invalid PcId in JWT token."
            });
        }

        var menu =
            await _menuService.GetMenuAsync(
                pcId,
                cancellationToken);

        return Ok(menu);
    }
}