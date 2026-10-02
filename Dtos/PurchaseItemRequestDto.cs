using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class PurchaseItemRequestDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
}
