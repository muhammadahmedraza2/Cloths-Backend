namespace ClothingErp.Api.Dtos;

public class RegisterRequestDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public int PcId { get; set; }
}