using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class AddToCartRequestDto
{
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
}
