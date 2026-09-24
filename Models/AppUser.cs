using Microsoft.AspNetCore.Identity;

namespace ClothingErp.Api.Models;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public int PcId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Backward-compatible alias used by the existing ERP code.
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Username
    {
        get => UserName ?? string.Empty;
        set => UserName = value;
    }

    // Kept for backward compatibility with the existing menu/ERP data.
    // ASP.NET Identity roles are the authoritative authorization source.
    public string Role { get; set; } = "User";

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
