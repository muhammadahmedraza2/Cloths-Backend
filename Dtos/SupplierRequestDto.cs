using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class SupplierRequestDto
{
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
