using ClothingErp.Api.Data;
using ClothingErp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/admin/products/{productId:guid}/images")]
[Authorize(Roles = "Admin")]
public class ProductImageController : ControllerBase
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly AppDbContext _db;
    public ProductImageController(AppDbContext db) => _db = db;

    [HttpPost("upload")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> Upload(Guid productId, IFormFile file)
    {
        if (!await _db.Products.AnyAsync(x => x.Id == productId)) return NotFound();
        if (file is null || file.Length == 0) return BadRequest(new { message = "Image is required." });
        if (file.Length > 5_000_000) return BadRequest(new { message = "Maximum image size is 5 MB." });

        var ext = Path.GetExtension(file.FileName);
        if (!Allowed.Contains(ext)) return BadRequest(new { message = "Only JPG, JPEG, PNG and WEBP images are allowed." });

        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var path = Path.Combine(folder, name);

        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);

        var hasPrimary = await _db.ProductImages.AnyAsync(x => x.ProductId == productId && x.IsPrimary);
        var image = new ProductImage { ProductId = productId, ImageUrl = $"/uploads/products/{name}", IsPrimary = !hasPrimary };
        _db.ProductImages.Add(image);
        await _db.SaveChangesAsync();

        return Ok(image);
    }

    [HttpDelete("{imageId:guid}")]
    public async Task<IActionResult> Delete(Guid productId, Guid imageId)
    {
        var image = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId);
        if (image is null) return NotFound();
        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
