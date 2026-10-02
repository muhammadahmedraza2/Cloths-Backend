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
public class AdminAgeGroupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminAgeGroupsController(
        AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("age-groups")]
    public async Task<IActionResult> AddAgeGroup(
        [FromBody] AgeGroupRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Age group name is required."
            });
        }

        if (dto.MinAgeMonths < 0)
        {
            return BadRequest(new
            {
                message = "Minimum age cannot be negative."
            });
        }

        if (dto.MaxAgeMonths.HasValue &&
            dto.MaxAgeMonths.Value < dto.MinAgeMonths)
        {
            return BadRequest(new
            {
                message = "Maximum age cannot be less than minimum age."
            });
        }

        var ageGroup = new AgeGroup
        {
            Name = dto.Name.Trim(),
            MinAgeMonths = dto.MinAgeMonths,
            MaxAgeMonths = dto.MaxAgeMonths,
            IsActive = dto.IsActive
        };

        _db.AgeGroups.Add(ageGroup);

        await _db.SaveChangesAsync();

        return Ok(ageGroup);
    }

    [HttpPut("age-groups/{id:guid}")]
    public async Task<IActionResult> UpdateAgeGroup(
        Guid id,
        [FromBody] AgeGroupRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Age group name is required."
            });
        }

        if (dto.MinAgeMonths < 0)
        {
            return BadRequest(new
            {
                message = "Minimum age cannot be negative."
            });
        }

        if (dto.MaxAgeMonths.HasValue &&
            dto.MaxAgeMonths.Value < dto.MinAgeMonths)
        {
            return BadRequest(new
            {
                message = "Maximum age cannot be less than minimum age."
            });
        }

        var ageGroup =
            await _db.AgeGroups.FindAsync(id);

        if (ageGroup is null)
            return NotFound();

        ageGroup.Name = dto.Name.Trim();
        ageGroup.MinAgeMonths = dto.MinAgeMonths;
        ageGroup.MaxAgeMonths = dto.MaxAgeMonths;
        ageGroup.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return Ok(ageGroup);
    }

    [HttpDelete("age-groups/{id:guid}")]
    public async Task<IActionResult> DeleteAgeGroup(Guid id)
    {
        var ageGroup =
            await _db.AgeGroups.FindAsync(id);

        if (ageGroup is null)
            return NotFound();

        ageGroup.IsActive = false;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Age group deactivated."
        });
    }
}
