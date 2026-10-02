using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class AgeGroupRequestDto
{
    public string Name { get; set; } = "";
    public int? MinAgeMonths { get; set; }
    public int? MaxAgeMonths { get; set; }
    public bool IsActive { get; set; } = true;
}
