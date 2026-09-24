using System.Security.Claims;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize(Roles = "User")]
public class AddressesController : ControllerBase
{
    private readonly EcommerceService _service;
    public AddressesController(EcommerceService service) => _service = service;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet] public async Task<IActionResult> Get() => Ok(await _service.GetAddressesAsync(UserId));
    [HttpPost] public async Task<IActionResult> Add(AddressRequestDto dto) => Ok(await _service.AddAddressAsync(UserId, dto));
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id) { await _service.DeleteAddressAsync(UserId, id); return Ok(); }
}
