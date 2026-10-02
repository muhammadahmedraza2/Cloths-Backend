using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

/// <summary>Single Piece, 2-Piece, 3-Piece, 4-Piece, Complete Set, etc.</summary>
