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
public class AdminColorsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminColorsController(
        AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("colors")]
    public async Task<IActionResult> AddColor(
        [FromBody] ColorRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Color name is required."
            });
        }

        var color = new Color
        {
            Name = dto.Name.Trim(),
            HexCode = dto.HexCode?.Trim(),
            IsActive = dto.IsActive
        };

        _db.Colors.Add(color);

        await _db.SaveChangesAsync();

        return Ok(color);
    }

    [HttpPut("colors/{id:guid}")]
    public async Task<IActionResult> UpdateColor(
        Guid id,
        [FromBody] ColorRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Color name is required."
            });
        }

        var color =
            await _db.Colors.FindAsync(id);

        if (color is null)
            return NotFound();

        color.Name = dto.Name.Trim();
        color.HexCode = dto.HexCode?.Trim();
        color.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return Ok(color);
    }

    [HttpDelete("colors/{id:guid}")]
    public async Task<IActionResult> DeleteColor(Guid id)
    {
        var color =
            await _db.Colors.FindAsync(id);

        if (color is null)
            return NotFound();

        color.IsActive = false;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Color deactivated."
        });
    }
}
