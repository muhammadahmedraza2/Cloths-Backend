namespace ClothingErp.Api.Dtos;

public class CartItemDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Qty { get; set; }
}

public class CartSummaryDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public int TotalQty { get; set; }
    public decimal TotalAmount { get; set; }
}

public class AddToCartDto
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Qty { get; set; } = 1;
}

public class UpdateCartQtyDto
{
    public int Qty { get; set; }
}