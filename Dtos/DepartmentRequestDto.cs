using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class DepartmentRequestDto
{
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
