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
public class AdminPurchasesController : ControllerBase
{
    private readonly PurchaseService _service;

    public AdminPurchasesController(
        PurchaseService service)
    {
        _service = service;
    }

    [HttpPost("purchases")]
    public async Task<IActionResult> Purchase(
        [FromBody] PurchaseRequestDto dto)
    {
        var purchase =
            await _service.CreatePurchaseAsync(dto);

        return Ok(purchase);
    }
}