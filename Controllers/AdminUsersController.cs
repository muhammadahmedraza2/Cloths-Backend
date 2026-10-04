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
public class AdminUsersController : ControllerBase
{
    private readonly UserService _service;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public AdminUsersController(
        UserService service,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _service = service;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var result = await _service.GetUsersAsync();
        return Ok(result);
    }

    [HttpPatch("users/{id:guid}/active")]
    public async Task<IActionResult> Active(
        Guid id,
        [FromQuery] bool value)
    {
        await _service.SetUserActiveAsync(id, value);

        return Ok(new
        {
            message = "User status updated."
        });
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        [FromBody] AdminCreateUserRequestDto dto)
    {
        if (dto is null)
        {
            return BadRequest(new
            {
                message = "User data is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            return BadRequest(new
            {
                message = "Username is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest(new
            {
                message = "Full name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new
            {
                message = "Password is required."
            });
        }

        var targetRole =
            string.Equals(dto.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "User";

        if (!await _roleManager.RoleExistsAsync(targetRole))
        {
            return BadRequest(new
            {
                message = "Requested role is not configured."
            });
        }

        var existingUser =
            await _userManager.FindByNameAsync(dto.Username.Trim());

        if (existingUser is not null)
        {
            return BadRequest(new
            {
                message = "Username already exists."
            });
        }

        var user = new AppUser
        {
            UserName = dto.Username.Trim(),
            Email = dto.Email?.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            FullName = dto.FullName.Trim(),
            PcId = dto.PcId,
            Role = targetRole,
            IsActive = true,
            EmailConfirmed = true
        };

        var result =
            await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = string.Join(
                    " ",
                    result.Errors.Select(x => x.Description))
            });
        }

        var roleResult =
            await _userManager.AddToRoleAsync(user, targetRole);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(new
            {
                message = string.Join(
                    " ",
                    roleResult.Errors.Select(x => x.Description))
            });
        }

        return Ok(new
        {
            user.Id,
            user.UserName,
            Role = targetRole,
            message = "User created successfully."
        });
    }
}