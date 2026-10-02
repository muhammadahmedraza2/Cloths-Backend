using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class CategoryRequestDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
