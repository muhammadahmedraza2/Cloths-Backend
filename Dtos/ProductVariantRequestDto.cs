using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class ProductVariantRequestDto
{
    public Guid SizeId { get; set; }
    public Guid ColorId { get; set; }
    public string SKU { get; set; } = "";
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsActive { get; set; } = true;
}
