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
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly EcommerceService _service;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IWebHostEnvironment _environment;

    public AdminController(
        AppDbContext db,
        EcommerceService service,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IWebHostEnvironment environment)
    {
        _db = db;
        _service = service;
        _userManager = userManager;
        _roleManager = roleManager;
        _environment = environment;
    }

    // =========================================================
    // DASHBOARD
    // =========================================================

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var result = await _service.GetDashboardAsync();
        return Ok(result);
    }

    // =========================================================
    // USERS
    // =========================================================

    [HttpGet("users")]
    public async Task<IActionResult> Users()
    {
        var result = await _service.GetUsersAsync();
        return Ok(result);
    }

    [HttpPatch("users/{id:guid}/active")]
    public async Task<IActionResult> Active(
        Guid id,
        [FromQuery] bool value)
    {
        await _service.SetUserActiveAsync(id, value);

        return Ok(new
        {
            message = "User status updated."
        });
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(
        [FromBody] AdminCreateUserRequestDto dto)
    {
        if (dto is null)
        {
            return BadRequest(new
            {
                message = "User data is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            return BadRequest(new
            {
                message = "Username is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest(new
            {
                message = "Full name is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new
            {
                message = "Password is required."
            });
        }

        var targetRole =
            string.Equals(dto.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "User";

        if (!await _roleManager.RoleExistsAsync(targetRole))
        {
            return BadRequest(new
            {
                message = "Requested role is not configured."
            });
        }

        var existingUser =
            await _userManager.FindByNameAsync(dto.Username.Trim());

        if (existingUser is not null)
        {
            return BadRequest(new
            {
                message = "Username already exists."
            });
        }

        var user = new AppUser
        {
            UserName = dto.Username.Trim(),
            Email = dto.Email?.Trim(),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            FullName = dto.FullName.Trim(),
            PcId = dto.PcId,
            Role = targetRole,
            IsActive = true,
            EmailConfirmed = true
        };

        var result =
            await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = string.Join(
                    " ",
                    result.Errors.Select(x => x.Description))
            });
        }

        var roleResult =
            await _userManager.AddToRoleAsync(user, targetRole);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return BadRequest(new
            {
                message = string.Join(
                    " ",
                    roleResult.Errors.Select(x => x.Description))
            });
        }

        return Ok(new
        {
            user.Id,
            user.UserName,
            Role = targetRole,
            message = "User created successfully."
        });
    }

    // =========================================================
    // ORDERS
    // =========================================================

    [HttpGet("orders")]
    public async Task<IActionResult> Orders()
    {
        var result = await _service.GetAllOrdersAsync();
        return Ok(result);
    }

    [HttpGet("orders/{id:guid}")]
    public async Task<IActionResult> Order(Guid id)
    {
        var result =
            await _service.GetOrderAsync(
                Guid.Empty,
                id,
                true);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPatch("orders/{id:guid}/status")]
    public async Task<IActionResult> OrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusDto dto)
    {
        await _service.UpdateOrderStatusAsync(
            id,
            dto.Status);

        return Ok(new
        {
            message = "Order status updated."
        });
    }

    // =========================================================
    // PAYMENTS
    // =========================================================

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

    // =========================================================
    // CATEGORIES
    // =========================================================

    [HttpPost("categories")]
    public async Task<IActionResult> AddCategory(
        [FromBody] CategoryRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            ImageUrl = dto.ImageUrl?.Trim(),
            IsActive = dto.IsActive
        };

        _db.Categories.Add(category);

        await _db.SaveChangesAsync();

        return Ok(category);
    }

    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] CategoryRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Category name is required."
            });
        }

        var category =
            await _db.Categories.FindAsync(id);

        if (category is null)
            return NotFound();

        category.Name = dto.Name.Trim();
        category.Description = dto.Description?.Trim();
        category.ImageUrl = dto.ImageUrl?.Trim();
        category.IsActive = dto.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(category);
    }

    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var category =
            await _db.Categories.FindAsync(id);

        if (category is null)
            return NotFound();

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Category deactivated."
        });
    }

    // =========================================================
    // BRANDS
    // =========================================================

    [HttpPost("brands")]
    public async Task<IActionResult> AddBrand(
        [FromBody] BrandRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Brand name is required."
            });
        }

        var brand = new Brand
        {
            Name = dto.Name.Trim(),
            IsActive = dto.IsActive
        };

        _db.Brands.Add(brand);

        await _db.SaveChangesAsync();

        return Ok(brand);
    }

    [HttpPut("brands/{id:guid}")]
    public async Task<IActionResult> UpdateBrand(
        Guid id,
        [FromBody] BrandRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Brand name is required."
            });
        }

        var brand =
            await _db.Brands.FindAsync(id);

        if (brand is null)
            return NotFound();

        brand.Name = dto.Name.Trim();
        brand.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return Ok(brand);
    }

    [HttpDelete("brands/{id:guid}")]
    public async Task<IActionResult> DeleteBrand(Guid id)
    {
        var brand =
            await _db.Brands.FindAsync(id);

        if (brand is null)
            return NotFound();

        brand.IsActive = false;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Brand deactivated."
        });
    }

    // =========================================================
    // SIZES
    // =========================================================

    [HttpPost("sizes")]
    public async Task<IActionResult> AddSize(
        [FromBody] SizeRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Size name is required."
            });
        }

        var size = new Size
        {
            Name = dto.Name.Trim(),
            AgeRange = dto.AgeRange?.Trim(),
            IsActive = dto.IsActive
        };

        _db.Sizes.Add(size);

        await _db.SaveChangesAsync();

        return Ok(size);
    }

    [HttpPut("sizes/{id:guid}")]
    public async Task<IActionResult> UpdateSize(
        Guid id,
        [FromBody] SizeRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Size name is required."
            });
        }

        var size =
            await _db.Sizes.FindAsync(id);

        if (size is null)
            return NotFound();

        size.Name = dto.Name.Trim();
        size.AgeRange = dto.AgeRange?.Trim();
        size.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return Ok(size);
    }

    [HttpDelete("sizes/{id:guid}")]
    public async Task<IActionResult> DeleteSize(Guid id)
    {
        var size =
            await _db.Sizes.FindAsync(id);

        if (size is null)
            return NotFound();

        size.IsActive = false;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Size deactivated."
        });
    }

    // =========================================================
    // COLORS
    // =========================================================

    [HttpPost("colors")]
    public async Task<IActionResult> AddColor(
        [FromBody] ColorRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Color name is required."
            });
        }

        var color = new Color
        {
            Name = dto.Name.Trim(),
            HexCode = dto.HexCode?.Trim(),
            IsActive = dto.IsActive
        };

        _db.Colors.Add(color);

        await _db.SaveChangesAsync();

        return Ok(color);
    }

    [HttpPut("colors/{id:guid}")]
    public async Task<IActionResult> UpdateColor(
        Guid id,
        [FromBody] ColorRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Color name is required."
            });
        }

        var color =
            await _db.Colors.FindAsync(id);

        if (color is null)
            return NotFound();

        color.Name = dto.Name.Trim();
        color.HexCode = dto.HexCode?.Trim();
        color.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return Ok(color);
    }

    [HttpDelete("colors/{id:guid}")]
    public async Task<IActionResult> DeleteColor(Guid id)
    {
        var color =
            await _db.Colors.FindAsync(id);

        if (color is null)
            return NotFound();

        color.IsActive = false;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Color deactivated."
        });
    }

    // =========================================================
    // AGE GROUPS
    // =========================================================

    [HttpPost("age-groups")]
    public async Task<IActionResult> AddAgeGroup(
        [FromBody] AgeGroupRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Age group name is required."
            });
        }

        if (dto.MinAgeMonths < 0)
        {
            return BadRequest(new
            {
                message = "Minimum age cannot be negative."
            });
        }

        if (dto.MaxAgeMonths.HasValue &&
            dto.MaxAgeMonths.Value < dto.MinAgeMonths)
        {
            return BadRequest(new
            {
                message = "Maximum age cannot be less than minimum age."
            });
        }

        var ageGroup = new AgeGroup
        {
            Name = dto.Name.Trim(),
            MinAgeMonths = dto.MinAgeMonths,
            MaxAgeMonths = dto.MaxAgeMonths,
            IsActive = dto.IsActive
        };

        _db.AgeGroups.Add(ageGroup);

        await _db.SaveChangesAsync();

        return Ok(ageGroup);
    }

    [HttpPut("age-groups/{id:guid}")]
    public async Task<IActionResult> UpdateAgeGroup(
        Guid id,
        [FromBody] AgeGroupRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new
            {
                message = "Age group name is required."
            });
        }

        if (dto.MinAgeMonths < 0)
        {
            return BadRequest(new
            {
                message = "Minimum age cannot be negative."
            });
        }

        if (dto.MaxAgeMonths.HasValue &&
            dto.MaxAgeMonths.Value < dto.MinAgeMonths)
        {
            return BadRequest(new
            {
                message = "Maximum age cannot be less than minimum age."
            });
        }

        var ageGroup =
            await _db.AgeGroups.FindAsync(id);

        if (ageGroup is null)
            return NotFound();

        ageGroup.Name = dto.Name.Trim();
        ageGroup.MinAgeMonths = dto.MinAgeMonths;
        ageGroup.MaxAgeMonths = dto.MaxAgeMonths;
        ageGroup.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return Ok(ageGroup);
    }

    [HttpDelete("age-groups/{id:guid}")]
    public async Task<IActionResult> DeleteAgeGroup(Guid id)
    {
        var ageGroup =
            await _db.AgeGroups.FindAsync(id);

        if (ageGroup is null)
            return NotFound();

        ageGroup.IsActive = false;

        await _db.SaveChangesAsync();

        return Ok(new
        {
            message = "Age group deactivated."
        });
    }

    // =========================================================
    // PRODUCTS
    // =========================================================

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

    // =========================================================
    // PRODUCT IMAGES
    // =========================================================

    [HttpPost("product-images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadProductImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                message = "Image file is required."
            });
        }

        var allowedExtensions = new[]
        {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                message = "Only JPG, JPEG, PNG and WEBP images are allowed."
            });
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new
            {
                message = "Image size cannot exceed 5 MB."
            });
        }

        var webRootPath = _environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot");
        }

        var uploadsFolder = Path.Combine(
            webRootPath,
            "uploads",
            "products");

        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid():N}{extension}";

        var filePath = Path.Combine(
            uploadsFolder,
            fileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.Create);

        await file.CopyToAsync(stream);

        var imageUrl =
            $"{Request.Scheme}://{Request.Host}/uploads/products/{fileName}";

        return Ok(new
        {
            url = imageUrl
        });
    }
    // =========================================================
    // STOCK
    // =========================================================

    [HttpGet("stock/{variantId:guid}/history")]
    public async Task<IActionResult> StockHistory(Guid variantId)
    {
        var result =
            await _service.GetStockHistoryAsync(
                variantId);

        return Ok(result);
    }

    // =========================================================
    // SUPPLIERS
    // =========================================================

    [HttpGet("suppliers")]
    public async Task<IActionResult> Suppliers()
    {
        var result =
            await _service.GetSuppliersAsync();

        return Ok(result);
    }

    [HttpPost("suppliers")]
    public async Task<IActionResult> AddSupplier(
        [FromBody] SupplierRequestDto dto)
    {
        var supplier =
            await _service.SaveSupplierAsync(
                null,
                dto);

        return Ok(supplier);
    }

    [HttpPut("suppliers/{id:guid}")]
    public async Task<IActionResult> UpdateSupplier(
        Guid id,
        [FromBody] SupplierRequestDto dto)
    {
        var supplier =
            await _service.SaveSupplierAsync(
                id,
                dto);

        return Ok(supplier);
    }

    // =========================================================
    // PURCHASES
    // =========================================================

    [HttpPost("purchases")]
    public async Task<IActionResult> Purchase(
        [FromBody] PurchaseRequestDto dto)
    {
        var purchase =
            await _service.CreatePurchaseAsync(dto);

        return Ok(purchase);
    }
}