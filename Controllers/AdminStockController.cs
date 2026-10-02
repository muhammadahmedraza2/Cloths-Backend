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
public class AdminStockController : ControllerBase
{
    private readonly EcommerceService _service;

    public AdminStockController(
        EcommerceService service)
    {
        _service = service;
    }

    [HttpGet("stock/{variantId:guid}/history")]
    public async Task<IActionResult> StockHistory(Guid variantId)
    {
        var result =
            await _service.GetStockHistoryAsync(
                variantId);

        return Ok(result);
    }
}
