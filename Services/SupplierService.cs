using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class SupplierService
{
    private readonly AppDbContext _db;

    public SupplierService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Supplier>> GetSuppliersAsync() =>
        _db.Suppliers
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

    public async Task<Supplier> SaveSupplierAsync(Guid? id, SupplierRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Supplier name is required.");

        Supplier supplier;

        if (id.HasValue)
        {
            supplier = await _db.Suppliers
                .FirstOrDefaultAsync(x => x.Id == id.Value)
                ?? throw new KeyNotFoundException("Supplier not found.");
        }
        else
        {
            supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            await _db.Suppliers.AddAsync(supplier);
        }

        supplier.Name = dto.Name.Trim();
        supplier.Phone = dto.Phone?.Trim();
        supplier.Email = dto.Email?.Trim();
        supplier.Address = dto.Address?.Trim();
        supplier.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        return supplier;
    }
}