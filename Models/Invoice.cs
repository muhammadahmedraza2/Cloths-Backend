using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Issued";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
