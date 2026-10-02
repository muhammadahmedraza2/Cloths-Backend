using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class CartItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = "";
    public string SKU { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string Size { get; set; } = "";
    public string Color { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
}
