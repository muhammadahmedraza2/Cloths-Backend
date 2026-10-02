using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class PurchaseRequestDto
{
    public Guid SupplierId { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public PurchasePaymentStatus PaymentStatus { get; set; }
    public string? Notes { get; set; }
    public List<PurchaseItemRequestDto> Items { get; set; } = new();
}
