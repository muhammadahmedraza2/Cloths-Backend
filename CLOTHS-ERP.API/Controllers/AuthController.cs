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

    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    // =========================================================
    // LOGIN
    // POST: /api/Auth/login
    // =========================================================
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginRequestDto dto)
    {
        var result =
            await _authService.LoginAsync(
                dto.Username,
                dto.Password);

        if (result is null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid username or password."
            });
        }

        return Ok(result);
    }


    // =========================================================
    // REGISTER
    // POST: /api/Auth/register
    // =========================================================
    [HttpPost("register")]
    public async Task<ActionResult> Register(
        RegisterRequestDto dto)
    {
        var result =
            await _authService.RegisterAsync(dto);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }

        return Ok(new
        {
            message = result.Message
        });
    }
}