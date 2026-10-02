using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public Guid? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public Gender Gender { get; set; } = Gender.Unisex;
    public Guid? AgeGroupId { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid? SetTypeId { get; set; }
    public SetType? SetType { get; set; }
    /// <summary>e.g. "Shirt + Trouser + Dupatta"</summary>
    public string? SetIncludes { get; set; }
    public string? Fabric { get; set; }
    public string? Season { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Discount { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
