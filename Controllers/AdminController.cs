using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using ClothingErp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly EcommerceService _service;
    public AdminController(AppDbContext db, EcommerceService service) { _db = db; _service = service; }

    [HttpGet("dashboard")] public async Task<IActionResult> Dashboard() => Ok(await _service.GetDashboardAsync());

    [HttpGet("users")] public async Task<IActionResult> Users() => Ok(await _service.GetUsersAsync());
    [HttpPatch("users/{id:guid}/active")] public async Task<IActionResult> Active(Guid id, [FromQuery] bool value) { await _service.SetUserActiveAsync(id, value); return Ok(new { message = "User status updated." }); }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(AdminCreateUserRequestDto dto)
    {
        // Reuse Identity service through the public registration rules, but role assignment is admin-only.
        var user = new AppUser { UserName = dto.Username.Trim(), Email = dto.Email?.Trim(), PhoneNumber = dto.PhoneNumber?.Trim(), FullName = dto.FullName.Trim(), PcId = dto.PcId, Role = dto.Role == "Admin" ? "Admin" : "User", IsActive = true, EmailConfirmed = true };
        var manager = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AppUser>>();
        var roles = HttpContext.RequestServices.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>>();
        var targetRole = dto.Role == "Admin" ? "Admin" : "User";
        if (!await roles.RoleExistsAsync(targetRole)) return BadRequest(new { message = "Requested role is not configured." });
        var result = await manager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(new { message = string.Join(" ", result.Errors.Select(x => x.Description)) });
        await manager.AddToRoleAsync(user, targetRole);
        return Ok(new { user.Id, user.UserName, Role = targetRole, message = "User created successfully." });
    }

    [HttpGet("orders")] public async Task<IActionResult> Orders() => Ok(await _service.GetAllOrdersAsync());
    [HttpGet("orders/{id:guid}")] public async Task<IActionResult> Order(Guid id) => (await _service.GetOrderAsync(Guid.Empty, id, true)) is { } o ? Ok(o) : NotFound();
    [HttpPatch("orders/{id:guid}/status")] public async Task<IActionResult> OrderStatus(Guid id, UpdateOrderStatusDto dto) { await _service.UpdateOrderStatusAsync(id, dto.Status); return Ok(new { message = "Order status updated." }); }

    [HttpGet("payments")] public async Task<IActionResult> Payments() => Ok(await _service.GetPaymentsAsync());
    [HttpPatch("payments/{id:guid}/status")] public async Task<IActionResult> PaymentStatus(Guid id, UpdatePaymentStatusDto dto) { await _service.UpdatePaymentStatusAsync(id, dto.Status); return Ok(new { message = "Payment status updated." }); }

    [HttpPost("categories")] public async Task<IActionResult> AddCategory(CategoryRequestDto dto) { var x = new Category { Name = dto.Name.Trim(), Description = dto.Description, ImageUrl = dto.ImageUrl, IsActive = dto.IsActive }; _db.Categories.Add(x); await _db.SaveChangesAsync(); return Ok(x); }
    [HttpPut("categories/{id:guid}")] public async Task<IActionResult> UpdateCategory(Guid id, CategoryRequestDto dto) { var x = await _db.Categories.FindAsync(id); if (x is null) return NotFound(); x.Name = dto.Name.Trim(); x.Description = dto.Description; x.ImageUrl = dto.ImageUrl; x.IsActive = dto.IsActive; x.UpdatedAt = DateTime.UtcNow; await _db.SaveChangesAsync(); return Ok(x); }
    [HttpDelete("categories/{id:guid}")] public async Task<IActionResult> DeleteCategory(Guid id) { var x = await _db.Categories.FindAsync(id); if (x is null) return NotFound(); x.IsActive = false; await _db.SaveChangesAsync(); return Ok(); }

    [HttpPost("brands")] public async Task<IActionResult> AddBrand(BrandRequestDto dto) { var x = new Brand { Name = dto.Name.Trim(), IsActive = dto.IsActive }; _db.Brands.Add(x); await _db.SaveChangesAsync(); return Ok(x); }
    [HttpPut("brands/{id:guid}")] public async Task<IActionResult> UpdateBrand(Guid id, BrandRequestDto dto) { var x = await _db.Brands.FindAsync(id); if (x is null) return NotFound(); x.Name = dto.Name.Trim(); x.IsActive = dto.IsActive; await _db.SaveChangesAsync(); return Ok(x); }
    [HttpDelete("brands/{id:guid}")] public async Task<IActionResult> DeleteBrand(Guid id) { var x = await _db.Brands.FindAsync(id); if (x is null) return NotFound(); x.IsActive = false; await _db.SaveChangesAsync(); return Ok(); }

    [HttpPost("sizes")] public async Task<IActionResult> AddSize(SizeRequestDto dto) { var x = new Size { Name = dto.Name.Trim(), AgeRange = dto.AgeRange, IsActive = dto.IsActive }; _db.Sizes.Add(x); await _db.SaveChangesAsync(); return Ok(x); }
    [HttpPut("sizes/{id:guid}")] public async Task<IActionResult> UpdateSize(Guid id, SizeRequestDto dto) { var x = await _db.Sizes.FindAsync(id); if (x is null) return NotFound(); x.Name = dto.Name.Trim(); x.AgeRange = dto.AgeRange; x.IsActive = dto.IsActive; await _db.SaveChangesAsync(); return Ok(x); }
    [HttpDelete("sizes/{id:guid}")] public async Task<IActionResult> DeleteSize(Guid id) { var x = await _db.Sizes.FindAsync(id); if (x is null) return NotFound(); x.IsActive = false; await _db.SaveChangesAsync(); return Ok(); }

    [HttpPost("colors")] public async Task<IActionResult> AddColor(ColorRequestDto dto) { var x = new Color { Name = dto.Name.Trim(), HexCode = dto.HexCode, IsActive = dto.IsActive }; _db.Colors.Add(x); await _db.SaveChangesAsync(); return Ok(x); }
    [HttpPut("colors/{id:guid}")] public async Task<IActionResult> UpdateColor(Guid id, ColorRequestDto dto) { var x = await _db.Colors.FindAsync(id); if (x is null) return NotFound(); x.Name = dto.Name.Trim(); x.HexCode = dto.HexCode; x.IsActive = dto.IsActive; await _db.SaveChangesAsync(); return Ok(x); }
    [HttpDelete("colors/{id:guid}")] public async Task<IActionResult> DeleteColor(Guid id) { var x = await _db.Colors.FindAsync(id); if (x is null) return NotFound(); x.IsActive = false; await _db.SaveChangesAsync(); return Ok(); }

    [HttpPost("age-groups")] public async Task<IActionResult> AddAgeGroup(AgeGroupRequestDto dto) { var x = new AgeGroup { Name = dto.Name.Trim(), MinAgeMonths = dto.MinAgeMonths, MaxAgeMonths = dto.MaxAgeMonths, IsActive = dto.IsActive }; _db.AgeGroups.Add(x); await _db.SaveChangesAsync(); return Ok(x); }
    [HttpPut("age-groups/{id:guid}")] public async Task<IActionResult> UpdateAgeGroup(Guid id, AgeGroupRequestDto dto) { var x = await _db.AgeGroups.FindAsync(id); if (x is null) return NotFound(); x.Name = dto.Name.Trim(); x.MinAgeMonths = dto.MinAgeMonths; x.MaxAgeMonths = dto.MaxAgeMonths; x.IsActive = dto.IsActive; await _db.SaveChangesAsync(); return Ok(x); }
    [HttpDelete("age-groups/{id:guid}")] public async Task<IActionResult> DeleteAgeGroup(Guid id) { var x = await _db.AgeGroups.FindAsync(id); if (x is null) return NotFound(); x.IsActive = false; await _db.SaveChangesAsync(); return Ok(); }

    [HttpPost("products")] public async Task<IActionResult> AddProduct(ProductRequestDto dto) => Ok(await _service.SaveProductAsync(null, dto));
    [HttpPut("products/{id:guid}")] public async Task<IActionResult> UpdateProduct(Guid id, ProductRequestDto dto) => Ok(await _service.SaveProductAsync(id, dto));
    [HttpDelete("products/{id:guid}")] public async Task<IActionResult> DeleteProduct(Guid id) { await _service.DeleteProductAsync(id); return Ok(new { message = "Product deactivated." }); }

    [HttpGet("stock/{variantId:guid}/history")] public async Task<IActionResult> StockHistory(Guid variantId) => Ok(await _service.GetStockHistoryAsync(variantId));

    [HttpGet("suppliers")] public async Task<IActionResult> Suppliers() => Ok(await _service.GetSuppliersAsync());
    [HttpPost("suppliers")] public async Task<IActionResult> AddSupplier(SupplierRequestDto dto) => Ok(await _service.SaveSupplierAsync(null, dto));
    [HttpPut("suppliers/{id:guid}")] public async Task<IActionResult> UpdateSupplier(Guid id, SupplierRequestDto dto) => Ok(await _service.SaveSupplierAsync(id, dto));

    [HttpPost("purchases")] public async Task<IActionResult> Purchase(PurchaseRequestDto dto) => Ok(await _service.CreatePurchaseAsync(dto));
}
