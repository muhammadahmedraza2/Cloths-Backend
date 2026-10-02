using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly EcommerceService _service;

    public AdminOrdersController(
        EcommerceService service)
    {
        _service = service;
    }

    [HttpGet("orders")]
    public async Task<IActionResult> Orders()
    {
        var result = await _service.GetAllOrdersAsync();
        return Ok(result);
    }

    [HttpGet("orders/{id:guid}")]
    public async Task<IActionResult> Order(Guid id)
    {
        var result =
            await _service.GetOrderAsync(
                Guid.Empty,
                id,
                true);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPatch("orders/{id:guid}/status")]
    public async Task<IActionResult> OrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusDto dto)
    {
        await _service.UpdateOrderStatusAsync(
            id,
            dto.Status);

        return Ok(new
        {
            message = "Order status updated."
        });
    }
}
