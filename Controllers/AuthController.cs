using System.Security.Claims;
using ClothingErp.Api.Dtos;
using CLOTHS_ERP.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // =========================================================
    // ADMIN LOGIN
    // =========================================================

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto dto)
    {
        var result =
            await _auth.LoginAsync(dto);

        if (!result.Success)
        {
            return Unauthorized(
                new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
        }

        return Ok(
            new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = result.Message,
                Data = result.Data
            });
    }


    // =========================================================
    // ADMIN CREATE USER
    // =========================================================

    [Authorize(Roles = "Admin")]
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        RegisterRequestDto dto)
    {
        var result =
            await _auth.RegisterAsync(dto);

        if (!result.Success)
        {
            return BadRequest(
                new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
        }

        return Ok(
            new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = result.Message,
                Data = result.Data
            });
    }


    // =========================================================
    // REFRESH TOKEN
    // =========================================================

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequestDto dto)
    {
        var result =
            await _auth.RefreshAsync(dto);

        if (!result.Success)
        {
            return Unauthorized(
                new ApiResponse<object>
                {
                    Success = false,
                    Message = result.Message
                });
        }

        return Ok(
            new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = result.Message,
                Data = result.Data
            });
    }


    // =========================================================
    // LOGOUT
    // =========================================================

    [Authorize(Roles = "Admin")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var claim =
            User.FindFirst(
                ClaimTypes.NameIdentifier);

        if (claim is null ||
            !Guid.TryParse(
                claim.Value,
                out var userId))
        {
            return Unauthorized(
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid admin token."
                });
        }

        await _auth.LogoutAsync(userId);

        return Ok(
            new ApiResponse<object>
            {
                Success = true,
                Message = "Logged out successfully."
            });
    }
}