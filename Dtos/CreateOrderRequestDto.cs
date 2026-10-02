using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class CreateOrderRequestDto
{
    public Guid ShippingAddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal ShippingAmount { get; set; }
    public string? BankName { get; set; }
    public string? TransactionReference { get; set; }
    public string? PaymentProofUrl { get; set; }
    public Guid? PaymentProofId { get; set; }
}
