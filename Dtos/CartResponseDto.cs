using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class CartResponseDto
{
    public Guid CartId { get; set; }
    public List<CartItemResponseDto> Items { get; set; } = new();
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}
