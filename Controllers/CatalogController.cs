using ClothingErp.Api.Models;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly EcommerceService _service;

    public CatalogController(EcommerceService service)
    {
        _service = service;
    }

    // =========================================================
    // PRODUCTS
    // =========================================================

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
        var result =
            await _service.GetProductsAsync(
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
        var product =
            await _service.GetProductAsync(id);

        if (product is null)
            return NotFound();

        return Ok(product);
    }

    // =========================================================
    // CATEGORIES
    // =========================================================

    [AllowAnonymous]
    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var result =
            await _service.GetCategoriesAsync();

        return Ok(result);
    }

    // =========================================================
    // BRANDS
    // =========================================================

    [AllowAnonymous]
    [HttpGet("brands")]
    public async Task<IActionResult> Brands()
    {
        var result =
            await _service.GetBrandsAsync();

        return Ok(result);
    }

    // =========================================================
    // SIZES
    // =========================================================

    [AllowAnonymous]
    [HttpGet("sizes")]
    public async Task<IActionResult> Sizes()
    {
        var result =
            await _service.GetSizesAsync();

        return Ok(result);
    }

    // =========================================================
    // COLORS
    // =========================================================

    [AllowAnonymous]
    [HttpGet("colors")]
    public async Task<IActionResult> Colors()
    {
        var result =
            await _service.GetColorsAsync();

        return Ok(result);
    }

    // =========================================================
    // AGE GROUPS
    // =========================================================

    [AllowAnonymous]
    [HttpGet("age-groups")]
    public async Task<IActionResult> AgeGroups()
    {
        var result =
            await _service.GetAgeGroupsAsync();

        return Ok(result);
    }

    // =========================================================
    // DEPARTMENTS
    // =========================================================

    [AllowAnonymous]
    [HttpGet("departments")]
    public async Task<IActionResult> Departments()
    {
        var result =
            await _service.GetDepartmentsAsync();

        return Ok(result);
    }

    // =========================================================
    // SET TYPES
    // =========================================================

    [AllowAnonymous]
    [HttpGet("set-types")]
    public async Task<IActionResult> SetTypes()
    {
        var result =
            await _service.GetSetTypesAsync();

        return Ok(result);
    }
}