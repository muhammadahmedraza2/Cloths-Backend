using ClothingErp.Api.Dtos;
using ClothingErp.Api.Interfaces;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class CheckoutService : ICheckoutService
{
    private readonly IRepository<CartItem> _cartItems;
    private readonly IRepository<Order> _orders;

    public CheckoutService(IRepository<CartItem> cartItems, IRepository<Order> orders)
    {
        _cartItems = cartItems;
        _orders = orders;
    }

    public async Task<CheckoutResponseDto?> CheckoutAsync(Guid userId, CheckoutRequestDto dto)
    {
        var items = await _cartItems.GetAllAsync(c => c.UserId == userId);
        if (items.Count == 0) return null;

        var order = new Order
        {
            UserId = userId,
            OrderNo = $"ORD-{DateTime.UtcNow:yyMMddHHmmss}",
            PaymentMethod = dto.PaymentMethod,
            TotalAmount = items.Sum(c => c.Qty * c.Price),
            CreatedAt = DateTime.UtcNow,
            Items = items.Select(c => new OrderItem
            {
                ProductId = c.ProductId,
                Name = c.Name,
                Price = c.Price,
                Qty = c.Qty
            }).ToList()
        };

        await _orders.AddAsync(order);
        _cartItems.RemoveRange(items);
        await _orders.SaveChangesAsync();

        return new CheckoutResponseDto
        {
            OrderNo = order.OrderNo,
            TotalAmount = order.TotalAmount,
            PaymentMethod = order.PaymentMethod.ToString(),
            CreatedAt = order.CreatedAt
        };
    }

    public async Task<List<object>> GetOrdersAsync(Guid userId)
    {
        var orders = await _orders.GetAllAsync(
            o => o.UserId == userId,
            include: q => q.Include(o => o.Items));
        return orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => (object)new
            {
                o.OrderNo,
                PaymentMethod = o.PaymentMethod.ToString(),
                o.TotalAmount,
                o.CreatedAt,
                Items = o.Items.Select(i => new { i.Name, i.Price, i.Qty })
            })
            .ToList();
    }
}