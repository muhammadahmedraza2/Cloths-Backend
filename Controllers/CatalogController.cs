using ClothingErp.Api.Models;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly CatalogService _service;

    public CatalogController(CatalogService service)
    {
        _service = service;
    }

    // ================= PRODUCTS =================

    [AllowAnonymous]
    [HttpGet("products")]
    public async Task<IActionResult> Products(
        [FromQuery] string? search,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? categoryId,
        [FromQuery] Gender? gender,
        [FromQuery] Guid? ageGroupId,
        [FromQuery] Guid? sizeId,
        [FromQuery] Guid? colorId,
        [FromQuery] Guid? setTypeId)
    {
        var result = await _service.GetProductsAsync(
            search,
            departmentId,
            categoryId,
            gender,
            ageGroupId,
            sizeId,
            colorId,
            setTypeId);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("products/{id:guid}")]
    public async Task<IActionResult> Product(Guid id)
    {
        var product = await _service.GetProductAsync(id);

        if (product is null)
            return NotFound();

        return Ok(product);
    }

    // ================= LOOKUPS =================

    [AllowAnonymous]
    [HttpGet("categories")]
    public async Task<IActionResult> Categories() =>
        Ok(await _service.GetCategoriesAsync());

    [AllowAnonymous]
    [HttpGet("brands")]
    public async Task<IActionResult> Brands() =>
        Ok(await _service.GetBrandsAsync());

    [AllowAnonymous]
    [HttpGet("sizes")]
    public async Task<IActionResult> Sizes() =>
        Ok(await _service.GetSizesAsync());

    [AllowAnonymous]
    [HttpGet("colors")]
    public async Task<IActionResult> Colors() =>
        Ok(await _service.GetColorsAsync());

    [AllowAnonymous]
    [HttpGet("age-groups")]
    public async Task<IActionResult> AgeGroups() =>
        Ok(await _service.GetAgeGroupsAsync());

    [AllowAnonymous]
    [HttpGet("departments")]
    public async Task<IActionResult> Departments() =>
        Ok(await _service.GetDepartmentsAsync());

    [AllowAnonymous]
    [HttpGet("set-types")]
    public async Task<IActionResult> SetTypes() =>
        Ok(await _service.GetSetTypesAsync());
}