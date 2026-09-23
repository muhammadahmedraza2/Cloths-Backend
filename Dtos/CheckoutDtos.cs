using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class CheckoutRequestDto
{
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
}

public class CheckoutResponseDto
{
    public string OrderNo { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}