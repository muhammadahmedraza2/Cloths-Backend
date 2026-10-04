using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class AddressService
{
    private readonly AppDbContext _db;

    public AddressService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Address>> GetAddressesAsync(Guid userId) =>
        _db.Addresses
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<Address> AddAddressAsync(Guid userId, AddressRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AddressLine))
            throw new InvalidOperationException("Address is required.");

        if (string.IsNullOrWhiteSpace(dto.City))
            throw new InvalidOperationException("City is required.");

        if (dto.IsDefault)
        {
            await _db.Addresses
                .Where(x => x.UserId == userId)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDefault, false));
        }

        var hasAddress = await _db.Addresses.AnyAsync(x => x.UserId == userId);

        var address = new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AddressLine = dto.AddressLine.Trim(),
            City = dto.City.Trim(),
            Area = dto.Area?.Trim(),
            PostalCode = dto.PostalCode?.Trim(),
            Country = string.IsNullOrWhiteSpace(dto.Country)
                ? "Pakistan"
                : dto.Country.Trim(),
            IsDefault = dto.IsDefault || !hasAddress,
            CreatedAt = DateTime.UtcNow
        };

        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();

        return address;
    }

    public async Task DeleteAddressAsync(Guid userId, Guid id)
    {
        var address = await _db.Addresses
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (address is null)
            throw new KeyNotFoundException("Address not found.");

        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();
    }
}