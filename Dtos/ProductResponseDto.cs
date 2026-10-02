using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = "";
    public string SKU { get; set; } = "";
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Gender Gender { get; set; }
    public Guid? AgeGroupId { get; set; }
    public string? AgeGroupName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? SetTypeId { get; set; }
    public string? SetTypeName { get; set; }
    public string? SetIncludes { get; set; }
    public string? Fabric { get; set; }
    public string? Season { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Discount { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsActive { get; set; }
    public List<string> Images { get; set; } = new();
    public List<ProductVariantResponseDto> Variants { get; set; } = new();
}
