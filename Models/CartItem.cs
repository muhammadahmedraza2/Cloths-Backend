using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public class CartItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CartId { get; set; }
    public Cart? Cart { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    [NotMapped]
    public decimal Price { get => UnitPrice; set => UnitPrice = value; }
    [NotMapped]
    public int Qty { get => Quantity; set => Quantity = value; }
}
