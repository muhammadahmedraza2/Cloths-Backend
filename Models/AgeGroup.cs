using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public class AgeGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int? MinAgeMonths { get; set; }
    public int? MaxAgeMonths { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

/// <summary>Top-level shop grouping: Baby, Kids, Teens, Men, Women, Newborn, Winter, Summer.</summary>
