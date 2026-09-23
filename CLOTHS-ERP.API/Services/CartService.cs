using ClothingErp.Api.Dtos;
using ClothingErp.Api.Interfaces;
using ClothingErp.Api.Models;

namespace ClothingErp.Api.Services;

public class CartService : ICartService
{
    private readonly IRepository<CartItem> _cartItems;

    public CartService(IRepository<CartItem> cartItems)
    {
        _cartItems = cartItems;
    }

    public async Task<CartSummaryDto> GetCartAsync(Guid userId)
    {
        var items = await _cartItems.GetAllAsync(c => c.UserId == userId);
        return ToSummary(items);
    }

    /// <summary>Called when the user clicks a product image — adds it to their cart.</summary>
    public async Task<CartSummaryDto> AddItemAsync(Guid userId, AddToCartDto dto)
    {
        var existing = await _cartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);

        if (existing is not null)
        {
            existing.Qty += dto.Qty <= 0 ? 1 : dto.Qty;
            _cartItems.Update(existing);
        }
        else
        {
            await _cartItems.AddAsync(new CartItem
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Name = dto.Name,
                ImageUrl = dto.ImageUrl,
                Price = dto.Price,
                Qty = dto.Qty <= 0 ? 1 : dto.Qty
            });
        }

        await _cartItems.SaveChangesAsync();
        var items = await _cartItems.GetAllAsync(c => c.UserId == userId);
        return ToSummary(items);
    }

    public async Task<CartSummaryDto?> UpdateQtyAsync(Guid userId, Guid itemId, int qty)
    {
        var item = await _cartItems.FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId);
        if (item is null) return null;

        item.Qty = Math.Max(1, qty);
        _cartItems.Update(item);
        await _cartItems.SaveChangesAsync();

        var items = await _cartItems.GetAllAsync(c => c.UserId == userId);
        return ToSummary(items);
    }

    public async Task<CartSummaryDto> RemoveItemAsync(Guid userId, Guid itemId)
    {
        var item = await _cartItems.FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId);
        if (item is not null)
        {
            _cartItems.Remove(item);
            await _cartItems.SaveChangesAsync();
        }

        var items = await _cartItems.GetAllAsync(c => c.UserId == userId);
        return ToSummary(items);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var items = await _cartItems.GetAllAsync(c => c.UserId == userId);
        _cartItems.RemoveRange(items);
        await _cartItems.SaveChangesAsync();
    }

    private static CartSummaryDto ToSummary(List<CartItem> items) => new()
    {
        Items = items.Select(i => new CartItemDto
        {
            Id = i.Id.ToString(),
            ProductId = i.ProductId,
            Name = i.Name,
            ImageUrl = i.ImageUrl,
            Price = i.Price,
            Qty = i.Qty
        }).ToList(),
        TotalQty = items.Sum(i => i.Qty),
        TotalAmount = items.Sum(i => i.Qty * i.Price)
    };
}