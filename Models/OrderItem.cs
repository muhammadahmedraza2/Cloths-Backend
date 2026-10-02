using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    [NotMapped]
    public string ProductId { get => ProductVariantId.ToString(); set { if (Guid.TryParse(value, out var id)) ProductVariantId = id; } }
    [NotMapped]
    public string Name { get => ProductName; set => ProductName = value; }
    [NotMapped]
    public decimal Price { get => UnitPrice; set => UnitPrice = value; }

    [NotMapped]
    public int Qty { get => Quantity; set => Quantity = value; }
}
