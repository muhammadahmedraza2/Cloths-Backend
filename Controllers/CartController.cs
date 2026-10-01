using System.Security.Claims;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "User,Admin")]
public class CartController : ControllerBase
{
    private readonly EcommerceService _service;

    public CartController(EcommerceService service)
    {
        _service = service;
    }

    private Guid UserId
    {
        get
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out var id))
            {
                throw new UnauthorizedAccessException(
                    "User ID claim is missing or invalid."
                );
            }

            return id;
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(
            await _service.GetCartAsync(UserId)
        );
    }

    [HttpPost("items")]
    public async Task<IActionResult> Add(
        [FromBody] AddToCartRequestDto dto)
    {
        return Ok(
            await _service.AddToCartAsync(
                UserId,
                dto
            )
        );
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCartRequestDto dto)
    {
        return Ok(
            await _service.UpdateCartAsync(
                UserId,
                id,
                dto.Quantity
            )
        );
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> Remove(Guid id)
    {
        return Ok(
            await _service.RemoveCartItemAsync(
                UserId,
                id
            )
        );
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