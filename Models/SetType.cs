using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public class SetType
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int PieceCount { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
