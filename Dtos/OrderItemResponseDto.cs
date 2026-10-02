using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class OrderItemResponseDto
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = "";
    public string SKU { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
