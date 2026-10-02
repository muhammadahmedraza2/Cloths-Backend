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
public class AdminSizesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminSizesController(
        AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("sizes")]
    public async Task<IActionResult> AddSize(
        [FromBody] SizeRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Size name is required."
            });
        }

        var size = new Size
        {
            Name = dto.Name.Trim(),
            AgeRange = dto.AgeRange?.Trim(),
            IsActive = dto.IsActive
        };

        _db.Sizes.Add(size);

        await _db.SaveChangesAsync();

        return Ok(size);
    }

    [HttpPut("sizes/{id:guid}")]
    public async Task<IActionResult> UpdateSize(
        Guid id,
        [FromBody] SizeRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Size name is required."
            });
        }

        var size =
            await _db.Sizes.FindAsync(id);

        if (size is null)
            return NotFound();

        size.Name = dto.Name.Trim();
        size.AgeRange = dto.AgeRange?.Trim();
        size.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return Ok(size);
    }

    [HttpDelete("sizes/{id:guid}")]
    public async Task<IActionResult> DeleteSize(Guid id)
    {
        var size =
            await _db.Sizes.FindAsync(id);

        if (size is null)
            return NotFound();

        size.IsActive = false;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Size deactivated."
        });
    }
}
