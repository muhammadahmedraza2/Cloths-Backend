using ClothingErp.Api.Dtos;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        var result = await _auth.RegisterAsync(dto);
        return result.Success ? Ok(new ApiResponse<LoginResponseDto> { Success = true, Message = result.Message, Data = result.Data })
            : BadRequest(new ApiResponse<object> { Success = false, Message = result.Message });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        var result = await _auth.LoginAsync(dto);
        return result.Success ? Ok(new ApiResponse<LoginResponseDto> { Success = true, Message = result.Message, Data = result.Data })
            : Unauthorized(new ApiResponse<object> { Success = false, Message = result.Message });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto)
    {
        var result = await _auth.RefreshAsync(dto);
        return result.Success ? Ok(new ApiResponse<LoginResponseDto> { Success = true, Message = result.Message, Data = result.Data })
            : Unauthorized(new ApiResponse<object> { Success = false, Message = result.Message });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var id = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _auth.LogoutAsync(id);
        return Ok(new ApiResponse<object> { Success = true, Message = "Logged out successfully." });
    }
}
