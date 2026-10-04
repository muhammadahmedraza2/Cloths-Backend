using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class CartService
{
    private readonly AppDbContext _db;

    public CartService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CartResponseDto> GetCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await MapCartAsync(cart.Id);
    }

    public async Task<CartResponseDto> AddToCartAsync(
        Guid userId,
        AddToCartRequestDto dto)
    {
        if (dto.ProductVariantId == Guid.Empty)
            throw new InvalidOperationException("Product variant is required.");

        if (dto.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        var variant = await _db.ProductVariants
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductVariantId && x.IsActive);

        if (variant?.Product is null || !variant.Product.IsActive)
            throw new KeyNotFoundException("Product variant not found or inactive.");

        if (variant.StockQuantity < dto.Quantity)
            throw new InvalidOperationException("Insufficient stock.");

        var cart = await GetOrCreateCartAsync(userId);

        var item = await _db.CartItems.FirstOrDefaultAsync(x =>
            x.CartId == cart.Id &&
            x.ProductVariantId == dto.ProductVariantId);

        if (item is null)
        {
            item = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                UserId = userId,
                ProductVariantId = dto.ProductVariantId,
                Quantity = dto.Quantity,
                UnitPrice = variant.SalePrice,
                ProductId = variant.ProductId.ToString(),
                Name = variant.Product.ProductName,
                AddedAt = DateTime.UtcNow
            };

            await _db.CartItems.AddAsync(item);
        }
        else
        {
            var newQuantity = item.Quantity + dto.Quantity;

            if (variant.StockQuantity < newQuantity)
                throw new InvalidOperationException("Insufficient stock.");

            item.Quantity = newQuantity;
            item.UnitPrice = variant.SalePrice;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await MapCartAsync(cart.Id);
    }

    public async Task<CartResponseDto> UpdateCartAsync(
        Guid userId,
        Guid itemId,
        int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        var item = await _db.CartItems
            .Include(x => x.ProductVariant)
            .FirstOrDefaultAsync(x => x.Id == itemId && x.UserId == userId);

        if (item is null)
            throw new KeyNotFoundException("Cart item not found.");

        if (item.ProductVariant is null || !item.ProductVariant.IsActive)
            throw new KeyNotFoundException("Product variant not found or inactive.");

        if (item.ProductVariant.StockQuantity < quantity)
            throw new InvalidOperationException("Insufficient stock.");

        item.Quantity = quantity;
        item.UnitPrice = item.ProductVariant.SalePrice;

        await _db.SaveChangesAsync();

        return await MapCartAsync(item.CartId);
    }

    public async Task<CartResponseDto> RemoveCartItemAsync(
        Guid userId,
        Guid itemId)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(x => x.Id == itemId && x.UserId == userId);

        if (item is null)
            throw new KeyNotFoundException("Cart item not found.");

        var cartId = item.CartId;

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();

        return await MapCartAsync(cartId);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var items = await _db.CartItems
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (items.Count == 0)
            return;

        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    // ================= PRIVATE =================

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _db.Carts.FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart is not null)
            return cart;

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _db.Carts.AddAsync(cart);
        await _db.SaveChangesAsync();

        return cart;
    }

    private async Task<CartResponseDto> MapCartAsync(Guid cartId)
    {
        var cart = await _db.Carts
            .AsNoTracking()
            .Include(x => x.Items)
                .ThenInclude(x => x.ProductVariant)
                    .ThenInclude(x => x!.Product)
                        .ThenInclude(x => x!.Images)
            .Include(x => x.Items)
                .ThenInclude(x => x.ProductVariant)
                    .ThenInclude(x => x!.Size)
            .Include(x => x.Items)
                .ThenInclude(x => x.ProductVariant)
                    .ThenInclude(x => x!.Color)
            .FirstOrDefaultAsync(x => x.Id == cartId);

        if (cart is null)
        {
            return new CartResponseDto
            {
                CartId = cartId,
                Items = new List<CartItemResponseDto>(),
                TotalQuantity = 0,
                TotalAmount = 0
            };
        }

        var items = cart.Items
            .Where(x => x.ProductVariant != null && x.ProductVariant.Product != null)
            .Select(x => new CartItemResponseDto
            {
                Id = x.Id,
                ProductVariantId = x.ProductVariantId,
                ProductName = x.ProductVariant!.Product!.ProductName,
                SKU = x.ProductVariant.SKU,
                ImageUrl =
                    x.ProductVariant.Product!.Images
                        .FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                    ?? x.ProductVariant.Product!.Images
                        .FirstOrDefault()?.ImageUrl,
                Size = x.ProductVariant.Size?.Name ?? "",
                Color = x.ProductVariant.Color?.Name ?? "",
                UnitPrice = x.ProductVariant.SalePrice,
                Quantity = x.Quantity,
                Total = x.ProductVariant.SalePrice * x.Quantity
            })
            .ToList();

        return new CartResponseDto
        {
            CartId = cart.Id,
            Items = items,
            TotalQuantity = items.Sum(x => x.Quantity),
            TotalAmount = items.Sum(x => x.Total)
        };
    }
}