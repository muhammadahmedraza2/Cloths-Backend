using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class ProductVariantResponseDto
{
    public Guid Id { get; set; }
    public Guid SizeId { get; set; }
    public string SizeName { get; set; } = "";
    public Guid ColorId { get; set; }
    public string ColorName { get; set; } = "";
    public string SKU { get; set; } = "";
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsActive { get; set; }
}
