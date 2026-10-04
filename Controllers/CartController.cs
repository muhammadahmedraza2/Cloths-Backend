using System.Security.Claims;

using ClothingErp.Api.Dtos;
using ClothingErp.Api.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartService _service;

    public CartController(
        CartService service)
    {
        _service = service;
    }

    private Guid UserId
    {
        get
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("nameid");

            if (!Guid.TryParse(userId, out var id))
                throw new UnauthorizedAccessException(
                    "User ID claim is missing or invalid.");

            return id;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetCartAsync(UserId);

        return Ok(result);
    }

    [HttpPost("items")]
    public async Task<IActionResult> Add(
        [FromBody] AddToCartRequestDto dto)
    {
        var result = await _service.AddToCartAsync(UserId, dto);

        return Ok(result);
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCartRequestDto dto)
    {
        var result = await _service.UpdateCartAsync(
            UserId,
            id,
            dto.Quantity);

        return Ok(result);
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        var result = await _service.RemoveCartItemAsync(UserId, id);

        return Ok(result);
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        await _service.ClearCartAsync(UserId);

        return Ok(new
        {
            message = "Cart cleared."
        });
    }
}