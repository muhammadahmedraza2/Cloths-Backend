using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? TransactionReference { get; set; }
    public string? Provider { get; set; }
    public string? BankName { get; set; }
    public string? PaymentProofUrl { get; set; }
    public Guid? PaymentProofId { get; set; }
    public DateTime? PaidAt { get; set; }
}
