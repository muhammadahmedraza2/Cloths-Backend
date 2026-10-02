using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class ColorRequestDto
{
    public string Name { get; set; } = "";
    public string? HexCode { get; set; }
    public bool IsActive { get; set; } = true;
}
