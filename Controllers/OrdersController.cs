using System.Security.Claims;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "User")]
public class OrdersController : ControllerBase
{
    private readonly EcommerceService _service;
    public OrdersController(EcommerceService service) => _service = service;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost] public async Task<IActionResult> Create(CreateOrderRequestDto dto) => Ok(await _service.CreateOrderAsync(UserId, dto));
    [HttpGet] public async Task<IActionResult> Mine() => Ok(await _service.GetMyOrdersAsync(UserId));
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id) => (await _service.GetOrderAsync(UserId, id, false)) is { } x ? Ok(x) : NotFound();
}
