using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

/// <summary>Customer-facing product list wrapper — "Not Found" ke liye.</summary>
public class CatalogProductsResultDto
{
    public bool Found { get; set; }
    public string? Message { get; set; }
    public List<ProductResponseDto> Products { get; set; } = new();
}
