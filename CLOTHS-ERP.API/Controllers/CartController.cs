using System.Security.Claims;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User id claim missing."));

    [HttpGet]
    public async Task<ActionResult<CartSummaryDto>> GetCart() =>
        Ok(await _cartService.GetCartAsync(CurrentUserId));

    /// <summary>Called when the user clicks a product image — adds it to their cart.</summary>
    [HttpPost("items")]
    public async Task<ActionResult<CartSummaryDto>> AddItem(AddToCartDto dto) =>
        Ok(await _cartService.AddItemAsync(CurrentUserId, dto));

    [HttpPut("items/{id:guid}")]
    public async Task<ActionResult<CartSummaryDto>> UpdateQty(Guid id, UpdateCartQtyDto dto)
    {
        var result = await _cartService.UpdateQtyAsync(CurrentUserId, id, dto.Qty);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<ActionResult<CartSummaryDto>> RemoveItem(Guid id) =>
        Ok(await _cartService.RemoveItemAsync(CurrentUserId, id));

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(CurrentUserId);
        return NoContent();
    }
}