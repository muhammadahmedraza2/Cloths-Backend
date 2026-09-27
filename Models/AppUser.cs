using Microsoft.AspNetCore.Identity;

namespace ClothingErp.Api.Models;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public int PcId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Role { get; set; } = "User";

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}