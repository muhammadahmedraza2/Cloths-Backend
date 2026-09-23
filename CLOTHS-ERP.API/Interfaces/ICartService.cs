using ClothingErp.Api.Dtos;

namespace ClothingErp.Api.Interfaces;

public interface ICartService
{
    Task<CartSummaryDto> GetCartAsync(Guid userId);
    Task<CartSummaryDto> AddItemAsync(Guid userId, AddToCartDto dto);
    Task<CartSummaryDto?> UpdateQtyAsync(Guid userId, Guid itemId, int qty);
    Task<CartSummaryDto> RemoveItemAsync(Guid userId, Guid itemId);
    Task ClearCartAsync(Guid userId);
}