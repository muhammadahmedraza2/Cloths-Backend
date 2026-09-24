
using System.Security.Claims;
using ClothingErp.Api.Data;
using ClothingErp.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/payment-proof")]
[Authorize]
public class PaymentProofController : ControllerBase
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".pdf" };
    private readonly AppDbContext _db;

    public PaymentProofController(AppDbContext db) => _db = db;

    [HttpPost("upload")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (!User.IsInRole("User")) return Forbid();
        if (file is null || file.Length == 0) return BadRequest(new { message = "File is required." });
        if (file.Length > 5_000_000) return BadRequest(new { message = "Maximum file size is 5 MB." });

        var ext = Path.GetExtension(file.FileName);
        if (!Allowed.Contains(ext)) return BadRequest(new { message = "Only JPG, JPEG, PNG and PDF files are allowed." });

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "payment-proofs");
        Directory.CreateDirectory(folder);

        var safeName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var path = Path.Combine(folder, safeName);

        await using (var stream = System.IO.File.Create(path))
            await file.CopyToAsync(stream);

        var proof = new PaymentProof
        {
            UserId = userId,
            StoredFileName = safeName,
            OriginalFileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            Size = file.Length
        };

        _db.PaymentProofs.Add(proof);
        await _db.SaveChangesAsync();

        return Ok(new { paymentProofId = proof.Id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var proofQuery = _db.PaymentProofs.AsNoTracking().Where(x => x.Id == id);
        if (!User.IsInRole("Admin"))
            proofQuery = proofQuery.Where(x => x.UserId == userId);

        var proof = await proofQuery.FirstOrDefaultAsync();
        if (proof is null) return NotFound();

        var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "payment-proofs", proof.StoredFileName);
        if (!System.IO.File.Exists(path)) return NotFound();

        return PhysicalFile(path, proof.ContentType, proof.OriginalFileName);
    }
}
