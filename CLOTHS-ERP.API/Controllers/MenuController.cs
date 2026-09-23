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

    public MenuController(IMenuService menuService, IOptions<MenuOptions> options)
    {
        _menuService = menuService;
        _options = options.Value;
    }

    [HttpGet]
    public async Task<ActionResult<List<MenuNodeDto>>> GetMenu(CancellationToken cancellationToken)
    {
        // PC_ID comes from the authenticated user's token, never from the client.
        var pcIdValue = User.FindFirst(_options.PcIdClaimType)?.Value;

        if (!int.TryParse(pcIdValue, out var pcId))
            return Forbid();

        var menu = await _menuService.GetMenuAsync(pcId, cancellationToken);

        return Ok(menu);
    }
}