using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string? InvoiceNumber { get; set; }
    public Guid UserId { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
}
