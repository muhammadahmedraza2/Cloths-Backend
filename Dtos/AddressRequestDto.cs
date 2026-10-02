using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class AddressRequestDto
{
    public string AddressLine { get; set; } = "";
    public string City { get; set; } = "";
    public string? Area { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Pakistan";
    public bool IsDefault { get; set; }
}
