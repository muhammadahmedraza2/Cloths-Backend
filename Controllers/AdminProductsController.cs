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
public class AdminProductsController : ControllerBase
{
    private readonly EcommerceService _service;

    public AdminProductsController(
        EcommerceService service)
    {
        _service = service;
    }

    [HttpPost("products")]
    public async Task<IActionResult> AddProduct(
        [FromBody] ProductRequestDto dto)
    {
        var product =
            await _service.SaveProductAsync(
                null,
                dto);

        return Ok(product);
    }

    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] ProductRequestDto dto)
    {
        try
        {
            var product = await _service.SaveProductAsync(id, dto);

            return Ok(product);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entries = ex.Entries
                .Select(x => new
                {
                    entity = x.Metadata.ClrType.Name,
                    state = x.State.ToString()
                })
                .ToList();

            return BadRequest(new
            {
                message = "Concurrency error while updating product.",
                entities = entries
            });
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = "Database update failed.",
                detail = ex.InnerException?.Message ?? ex.Message
            });
        }
    }
    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _service.DeleteProductAsync(id);

        return Ok(new
        {
            message = "Product deactivated."
        });
    }
}
