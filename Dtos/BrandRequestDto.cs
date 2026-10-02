using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class BrandRequestDto
{
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
