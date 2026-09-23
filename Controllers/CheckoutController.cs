using System.Security.Claims;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/checkout")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User id claim missing."));

    /// <summary>
    /// Places an order from the current cart using the chosen payment method
    /// (Cash or Bank), then empties the cart.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CheckoutResponseDto>> Checkout(CheckoutRequestDto dto)
    {
        var result = await _checkoutService.CheckoutAsync(CurrentUserId, dto);
        if (result is null)
        {
            return BadRequest(new { message = "Your cart is empty." });
        }
        return Ok(result);
    }

    /// <summary>Order history for the current user.</summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders() =>
        Ok(await _checkoutService.GetOrdersAsync(CurrentUserId));
}