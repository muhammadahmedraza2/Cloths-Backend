using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly ClothingErp.Api.Data.AppDbContext _db;
    private readonly EcommerceService _service;
    public CatalogController(ClothingErp.Api.Data.AppDbContext db, EcommerceService service) { _db = db; _service = service; }

    [AllowAnonymous]
    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] string? search, [FromQuery] Guid? categoryId, [FromQuery] Guid? ageGroupId, [FromQuery] Guid? sizeId, [FromQuery] Guid? colorId)
        => Ok(await _service.GetProductsAsync(search, categoryId, ageGroupId, sizeId, colorId));

    [AllowAnonymous]
    [HttpGet("products/{id:guid}")]
    public async Task<IActionResult> Product(Guid id) => (await _service.GetProductAsync(id)) is { } p ? Ok(p) : NotFound();

    [AllowAnonymous]
    [HttpGet("categories")] public async Task<IActionResult> Categories() => Ok(await _db.Categories.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync());
    [AllowAnonymous]
    [HttpGet("brands")] public async Task<IActionResult> Brands() => Ok(await _db.Brands.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync());
    [AllowAnonymous]
    [HttpGet("sizes")] public async Task<IActionResult> Sizes() => Ok(await _db.Sizes.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync());
    [AllowAnonymous]
    [HttpGet("colors")] public async Task<IActionResult> Colors() => Ok(await _db.Colors.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync());
    [AllowAnonymous]
    [HttpGet("age-groups")] public async Task<IActionResult> AgeGroups() => Ok(await _db.AgeGroups.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync());
}
