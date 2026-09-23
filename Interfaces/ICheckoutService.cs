using ClothingErp.Api.Dtos;

namespace ClothingErp.Api.Interfaces;

public interface ICheckoutService
{
    Task<CheckoutResponseDto?> CheckoutAsync(Guid userId, CheckoutRequestDto dto);
    Task<List<object>> GetOrdersAsync(Guid userId);
}