using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;

namespace ClothingErp.Api.Services;

// Shared mapper: CatalogService aur ProductAdminService dono use karte hain
public static class ProductMapper
{
    public static ProductResponseDto MapProduct(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            ProductName = product.ProductName,
            SKU = product.SKU,
            Description = product.Description,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name,
            Gender = product.Gender,
            AgeGroupId = product.AgeGroupId,
            AgeGroupName = product.AgeGroup?.Name,
            DepartmentId = product.DepartmentId,
            DepartmentName = product.Department?.Name,
            SetTypeId = product.SetTypeId,
            SetTypeName = product.SetType?.Name,
            SetIncludes = product.SetIncludes,
            Fabric = product.Fabric,
            Season = product.Season,
            PurchasePrice = product.PurchasePrice,
            SalePrice = product.SalePrice,
            Discount = product.Discount,
            StockQuantity = product.Variants
                .Where(x => x.IsActive)
                .Sum(x => x.StockQuantity),
            MinimumStockLevel = product.MinimumStockLevel,
            IsActive = product.IsActive,
            Images = product.Images
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.CreatedAt)
                .Select(x => x.ImageUrl)
                .ToList(),
            Variants = product.Variants
                .Select(v => new ProductVariantResponseDto
                {
                    Id = v.Id,
                    SizeId = v.SizeId,
                    SizeName = v.Size?.Name ?? "",
                    ColorId = v.ColorId,
                    ColorName = v.Color?.Name ?? "",
                    SKU = v.SKU,
                    PurchasePrice = v.PurchasePrice,
                    SalePrice = v.SalePrice,
                    StockQuantity = v.StockQuantity,
                    MinimumStockLevel = v.MinimumStockLevel,
                    IsActive = v.IsActive
                })
                .ToList()
        };
    }
}