using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class ProductAdminService
{
    private readonly AppDbContext _db;
    private readonly CatalogService _catalog;

    public ProductAdminService(AppDbContext db, CatalogService catalog)
    {
        _db = db;
        _catalog = catalog;
    }

    // ================= SAVE / UPDATE =================

    public async Task<ProductResponseDto> SaveProductAsync(
        Guid? id,
        ProductRequestDto dto)
    {
        await ValidateProductAsync(id, dto);

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            Product product;

            if (id.HasValue)
            {
                product = await _db.Products
                    .Include(x => x.Variants)
                    .Include(x => x.Images)
                    .FirstOrDefaultAsync(x => x.Id == id.Value)
                    ?? throw new KeyNotFoundException("Product not found.");

                UpdateProduct(product, dto);

                // Variants delete nahi hote, sirf inactive hote hain
                UpdateProductVariants(product, dto);

                // Images abhi touch nahi hoti

                product.StockQuantity = product.Variants
                    .Where(x => x.IsActive)
                    .Sum(x => x.StockQuantity);

                await _db.SaveChangesAsync();
            }
            else
            {
                product = CreateProduct(dto);

                await _db.Products.AddAsync(product);

                AddProductVariants(product, dto);
                AddProductImages(product, dto);

                product.StockQuantity = product.Variants
                    .Where(x => x.IsActive)
                    .Sum(x => x.StockQuantity);

                await _db.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return await _catalog.GetProductAsync(product.Id)
                ?? throw new InvalidOperationException(
                    "Product was saved but could not be loaded.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);

        if (product is null)
            throw new KeyNotFoundException("Product not found.");

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // ================= VALIDATION =================

    private async Task ValidateProductAsync(Guid? id, ProductRequestDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException("Product data is required.");

        if (string.IsNullOrWhiteSpace(dto.ProductName))
            throw new InvalidOperationException("Product name is required.");

        if (string.IsNullOrWhiteSpace(dto.SKU))
            throw new InvalidOperationException("Product SKU is required.");

        if (dto.CategoryId == Guid.Empty)
            throw new InvalidOperationException("Category is required.");

        if (!await _db.Categories.AnyAsync(x => x.Id == dto.CategoryId && x.IsActive))
            throw new InvalidOperationException("Category does not exist or is inactive.");

        if (dto.BrandId.HasValue &&
            !await _db.Brands.AnyAsync(x => x.Id == dto.BrandId.Value && x.IsActive))
            throw new InvalidOperationException("Brand does not exist or is inactive.");

        if (dto.AgeGroupId.HasValue &&
            !await _db.AgeGroups.AnyAsync(x => x.Id == dto.AgeGroupId.Value && x.IsActive))
            throw new InvalidOperationException("Age group does not exist or is inactive.");

        if (dto.DepartmentId.HasValue &&
            !await _db.Departments.AnyAsync(x => x.Id == dto.DepartmentId.Value && x.IsActive))
            throw new InvalidOperationException("Department does not exist or is inactive.");

        if (dto.SetTypeId.HasValue &&
            !await _db.SetTypes.AnyAsync(x => x.Id == dto.SetTypeId.Value && x.IsActive))
            throw new InvalidOperationException("Set type does not exist or is inactive.");

        if (dto.PurchasePrice < 0)
            throw new InvalidOperationException("Purchase price cannot be negative.");

        if (dto.SalePrice < 0)
            throw new InvalidOperationException("Sale price cannot be negative.");

        if (dto.Discount < 0)
            throw new InvalidOperationException("Discount cannot be negative.");

        if (dto.MinimumStockLevel < 0)
            throw new InvalidOperationException("Minimum stock level cannot be negative.");

        var productSku = dto.SKU.Trim();

        var duplicateProductSku = await _db.Products.AnyAsync(x =>
            x.SKU == productSku &&
            (!id.HasValue || x.Id != id.Value));

        if (duplicateProductSku)
            throw new InvalidOperationException("Product SKU already exists.");

        if (dto.Variants is null)
            throw new InvalidOperationException("Product variants are required.");

        var variantSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var variant in dto.Variants)
        {
            if (variant.SizeId == Guid.Empty)
                throw new InvalidOperationException("Variant size is required.");

            if (variant.ColorId == Guid.Empty)
                throw new InvalidOperationException("Variant color is required.");

            if (string.IsNullOrWhiteSpace(variant.SKU))
                throw new InvalidOperationException("Variant SKU is required.");

            if (variant.PurchasePrice < 0)
                throw new InvalidOperationException("Variant purchase price cannot be negative.");

            if (variant.SalePrice < 0)
                throw new InvalidOperationException("Variant sale price cannot be negative.");

            if (variant.StockQuantity < 0)
                throw new InvalidOperationException("Variant stock quantity cannot be negative.");

            if (variant.MinimumStockLevel < 0)
                throw new InvalidOperationException("Variant minimum stock level cannot be negative.");

            var variantSku = variant.SKU.Trim();

            if (!variantSkus.Add(variantSku))
                throw new InvalidOperationException($"Duplicate variant SKU: {variantSku}");

            if (!await _db.Sizes.AnyAsync(x => x.Id == variant.SizeId && x.IsActive))
                throw new InvalidOperationException("Variant size does not exist or is inactive.");

            if (!await _db.Colors.AnyAsync(x => x.Id == variant.ColorId && x.IsActive))
                throw new InvalidOperationException("Variant color does not exist or is inactive.");

            var duplicateVariantSku = await _db.ProductVariants.AnyAsync(x =>
                x.SKU == variantSku &&
                (!id.HasValue || x.ProductId != id.Value));

            if (duplicateVariantSku)
                throw new InvalidOperationException($"Variant SKU already exists: {variantSku}");
        }

        if (id.HasValue)
        {
            var productExists = await _db.Products.AnyAsync(x => x.Id == id.Value);

            if (!productExists)
                throw new KeyNotFoundException("Product not found.");
        }
    }

    // ================= CREATE / UPDATE HELPERS =================

    private static Product CreateProduct(ProductRequestDto dto)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            ProductName = dto.ProductName.Trim(),
            SKU = dto.SKU.Trim(),
            Description = dto.Description?.Trim(),
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId,
            Gender = dto.Gender,
            AgeGroupId = dto.AgeGroupId,
            DepartmentId = dto.DepartmentId,
            SetTypeId = dto.SetTypeId,
            SetIncludes = dto.SetIncludes?.Trim(),
            Fabric = dto.Fabric?.Trim(),
            Season = dto.Season?.Trim(),
            PurchasePrice = dto.PurchasePrice,
            SalePrice = dto.SalePrice,
            Discount = dto.Discount,
            MinimumStockLevel = dto.MinimumStockLevel,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void UpdateProduct(Product product, ProductRequestDto dto)
    {
        product.ProductName = dto.ProductName.Trim();
        product.SKU = dto.SKU.Trim();
        product.Description = dto.Description?.Trim();
        product.CategoryId = dto.CategoryId;
        product.BrandId = dto.BrandId;
        product.Gender = dto.Gender;
        product.AgeGroupId = dto.AgeGroupId;
        product.DepartmentId = dto.DepartmentId;
        product.SetTypeId = dto.SetTypeId;
        product.SetIncludes = dto.SetIncludes?.Trim();
        product.Fabric = dto.Fabric?.Trim();
        product.Season = dto.Season?.Trim();
        product.PurchasePrice = dto.PurchasePrice;
        product.SalePrice = dto.SalePrice;
        product.Discount = dto.Discount;
        product.MinimumStockLevel = dto.MinimumStockLevel;
        product.IsActive = dto.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
    }

    private static void AddProductVariants(Product product, ProductRequestDto dto)
    {
        foreach (var variantDto in dto.Variants)
        {
            product.Variants.Add(new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                SizeId = variantDto.SizeId,
                ColorId = variantDto.ColorId,
                SKU = variantDto.SKU.Trim(),
                PurchasePrice = variantDto.PurchasePrice,
                SalePrice = variantDto.SalePrice,
                StockQuantity = variantDto.StockQuantity,
                MinimumStockLevel = variantDto.MinimumStockLevel,
                IsActive = variantDto.IsActive
            });
        }
    }

    private static void UpdateProductVariants(Product product, ProductRequestDto dto)
    {
        var incomingSkus = new HashSet<string>(
            dto.Variants
                .Where(x => !string.IsNullOrWhiteSpace(x.SKU))
                .Select(x => x.SKU.Trim()),
            StringComparer.OrdinalIgnoreCase);

        foreach (var variantDto in dto.Variants)
        {
            var sku = variantDto.SKU.Trim();

            var existingVariant = product.Variants.FirstOrDefault(x =>
                string.Equals(x.SKU, sku, StringComparison.OrdinalIgnoreCase));

            if (existingVariant != null)
            {
                existingVariant.SizeId = variantDto.SizeId;
                existingVariant.ColorId = variantDto.ColorId;
                existingVariant.SKU = sku;
                existingVariant.PurchasePrice = variantDto.PurchasePrice;
                existingVariant.SalePrice = variantDto.SalePrice;
                existingVariant.StockQuantity = variantDto.StockQuantity;
                existingVariant.MinimumStockLevel = variantDto.MinimumStockLevel;
                existingVariant.IsActive = variantDto.IsActive;
            }
            else
            {
                product.Variants.Add(new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    SizeId = variantDto.SizeId,
                    ColorId = variantDto.ColorId,
                    SKU = sku,
                    PurchasePrice = variantDto.PurchasePrice,
                    SalePrice = variantDto.SalePrice,
                    StockQuantity = variantDto.StockQuantity,
                    MinimumStockLevel = variantDto.MinimumStockLevel,
                    IsActive = variantDto.IsActive
                });
            }
        }

        // Jo variants request mein nahi aaye unko delete nahi, sirf inactive karna hai
        foreach (var existingVariant in product.Variants)
        {
            if (!incomingSkus.Contains(existingVariant.SKU))
                existingVariant.IsActive = false;
        }
    }

    private static void AddProductImages(Product product, ProductRequestDto dto)
    {
        var imageUrls = (dto.ImageUrls ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct()
            .ToList();

        for (int i = 0; i < imageUrls.Count; i++)
        {
            product.Images.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ImageUrl = imageUrls[i],
                IsPrimary = i == 0,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}