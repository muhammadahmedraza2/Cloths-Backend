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
    public ProfileController(UserManager<AppUser> users) => _users = users;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var u = await _users.FindByIdAsync(id.ToString());
        return u is null ? NotFound() : Ok(new UserResponseDto { Id = u.Id, Username = u.UserName ?? "", Email = u.Email, PhoneNumber = u.PhoneNumber, FullName = u.FullName, Role = "User", IsActive = u.IsActive });
    }

    [HttpPut]
    public async Task<IActionResult> Update(UserResponseDto dto)
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var u = await _users.FindByIdAsync(id.ToString());
        if (u is null) return NotFound();
        u.FullName = dto.FullName.Trim();
        u.Email = dto.Email?.Trim();
        u.PhoneNumber = dto.PhoneNumber?.Trim();
        var result = await _users.UpdateAsync(u);
        return result.Succeeded ? Ok(new { message = "Profile updated." }) : BadRequest(new { message = string.Join(" ", result.Errors.Select(x => x.Description)) });
    }
}
