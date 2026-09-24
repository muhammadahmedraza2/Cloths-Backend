using System.Security.Claims;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "User")]
public class CartController : ControllerBase
{
    private readonly EcommerceService _service;
    public CartController(EcommerceService service) => _service = service;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet] public async Task<IActionResult> Get() => Ok(await _service.GetCartAsync(UserId));
    [HttpPost("items")] public async Task<IActionResult> Add(AddToCartRequestDto dto) => Ok(await _service.AddToCartAsync(UserId, dto));
    [HttpPut("items/{id:guid}")] public async Task<IActionResult> Update(Guid id, UpdateCartRequestDto dto) => Ok(await _service.UpdateCartAsync(UserId, id, dto.Quantity));
    [HttpDelete("items/{id:guid}")] public async Task<IActionResult> Remove(Guid id) => Ok(await _service.RemoveCartItemAsync(UserId, id));
    [HttpDelete("clear")] public async Task<IActionResult> Clear() { await _service.ClearCartAsync(UserId); return Ok(new { message = "Cart cleared." }); }
}
