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
public class AdminSuppliersController : ControllerBase
{
    private readonly EcommerceService _service;

    public AdminSuppliersController(
        EcommerceService service)
    {
        _service = service;
    }

    [HttpGet("suppliers")]
    public async Task<IActionResult> Suppliers()
    {
        var result =
            await _service.GetSuppliersAsync();

        return Ok(result);
    }

    [HttpPost("suppliers")]
    public async Task<IActionResult> AddSupplier(
        [FromBody] SupplierRequestDto dto)
    {
        var supplier =
            await _service.SaveSupplierAsync(
                null,
                dto);

        return Ok(supplier);
    }

    [HttpPut("suppliers/{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(
        Guid id,
        [FromBody] SupplierRequestDto dto)
    {
        var supplier =
            await _service.SaveSupplierAsync(
                id,
                dto);

        return Ok(supplier);
    }
}
