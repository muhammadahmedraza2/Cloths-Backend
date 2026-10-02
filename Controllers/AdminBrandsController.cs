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
public class AdminBrandsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminBrandsController(
        AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("brands")]
    public async Task<IActionResult> AddBrand(
        [FromBody] BrandRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Brand name is required."
            });
        }

        var brand = new Brand
        {
            Name = dto.Name.Trim(),
            IsActive = dto.IsActive
        };

        _db.Brands.Add(brand);

        await _db.SaveChangesAsync();

        return Ok(brand);
    }

    [HttpPut("brands/{id:guid}")]
    public async Task<IActionResult> UpdateBrand(
        Guid id,
        [FromBody] BrandRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Brand name is required."
            });
        }

        var brand =
            await _db.Brands.FindAsync(id);

        if (brand is null)
            return NotFound();

        brand.Name = dto.Name.Trim();
        brand.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return Ok(brand);
    }

    [HttpDelete("brands/{id:guid}")]
    public async Task<IActionResult> DeleteBrand(Guid id)
    {
        var brand =
            await _db.Brands.FindAsync(id);

        if (brand is null)
            return NotFound();

        brand.IsActive = false;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Brand deactivated."
        });
    }
}
