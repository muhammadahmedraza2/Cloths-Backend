using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class SizeRequestDto
{
    public string Name { get; set; } = "";
    public string? AgeRange { get; set; }
    public bool IsActive { get; set; } = true;
}
