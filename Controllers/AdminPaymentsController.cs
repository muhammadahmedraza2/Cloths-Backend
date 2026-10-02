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
public class AdminPaymentsController : ControllerBase
{
    private readonly EcommerceService _service;

    public AdminPaymentsController(
        EcommerceService service)
    {
        _service = service;
    }

    [HttpGet("payments")]
    public async Task<IActionResult> Payments()
    {
        var result = await _service.GetPaymentsAsync();
        return Ok(result);
    }

    [HttpPatch("payments/{id:guid}/status")]
    public async Task<IActionResult> PaymentStatus(
        Guid id,
        [FromBody] UpdatePaymentStatusDto dto)
    {
        await _service.UpdatePaymentStatusAsync(
            id,
            dto.Status);

        return Ok(new
        {
            message = "Payment status updated."
        });
    }
}
