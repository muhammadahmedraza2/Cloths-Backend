using ClothingErp.Api.Dtos;
using ClothingErp.Api.Interfaces;
using CLOTHS_ERP.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Seeded demo user: username "admin", password "Admin@123".
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto.Username, dto.Password);
        if (result is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }
        return Ok(result);
    }
}