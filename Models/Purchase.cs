using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public class Purchase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string PurchaseNumber { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public PurchasePaymentStatus PaymentStatus { get; set; } = PurchasePaymentStatus.Pending;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
}
