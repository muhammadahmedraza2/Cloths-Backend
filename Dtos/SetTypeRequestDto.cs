using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class SetTypeRequestDto
{
    public string Name { get; set; } = "";
    public int PieceCount { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
