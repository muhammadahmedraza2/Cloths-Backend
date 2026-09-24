using ClothingErp.Api.Dtos;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize(Roles = "Admin")]
public class PaymentsController : ControllerBase
{
    private readonly EcommerceService _service;
    public PaymentsController(EcommerceService service) => _service = service;

    [HttpGet] public async Task<IActionResult> Get() => Ok(await _service.GetPaymentsAsync());
    [HttpPatch("{id:guid}/status")] public async Task<IActionResult> Status(Guid id, UpdatePaymentStatusDto dto) { await _service.UpdatePaymentStatusAsync(id, dto.Status); return Ok(new { message = "Payment status updated." }); }
}
