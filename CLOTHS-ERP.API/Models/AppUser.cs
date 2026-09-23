namespace ClothingErp.Api.Models;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    // PC_ID used for menu permissions
    public int PcId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

