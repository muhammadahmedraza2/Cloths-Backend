using System.Security.Claims;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize(Roles = "User")]
public class ProfileController : ControllerBase
{
    private readonly UserManager<AppUser> _users;

    public ProfileController(
        UserManager<AppUser> users)
    {
        _users = users;
    }

    // =========================================================
    // GET PROFILE
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            claim,
            out var id))
        {
            return Unauthorized();
        }

        var user =
            await _users.FindByIdAsync(
                id.ToString());

        if (user is null)
        {
            return NotFound();
        }

        return Ok(
            new UserResponseDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                Role = "User"
            });
    }


    // =========================================================
    // UPDATE PROFILE
    // =========================================================

    [HttpPut]
    public async Task<IActionResult> Update(
        UserResponseDto dto)
    {
        var claim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
            claim,
            out var id))
        {
            return Unauthorized();
        }

        var user =
            await _users.FindByIdAsync(
                id.ToString());

        if (user is null)
        {
            return NotFound();
        }

        user.FullName =
            dto.FullName.Trim();

        user.Email =
            string.IsNullOrWhiteSpace(dto.Email)
                ? null
                : dto.Email.Trim();

        user.PhoneNumber =
            string.IsNullOrWhiteSpace(dto.PhoneNumber)
                ? null
                : dto.PhoneNumber.Trim();

        var result =
            await _users.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(
                new
                {
                    message = string.Join(
                        " ",
                        result.Errors.Select(
                            x => x.Description))
                });
        }

        return Ok(
            new
            {
                message = "Profile updated."
            });
    }
}