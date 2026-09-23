namespace ClothingErp.Api.Models;

public class CartItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public AppUser? User { get; set; }

    /// <summary>References MasterRecord.Id (the product/order row the image was clicked on).</summary>
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Qty { get; set; } = 1;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}