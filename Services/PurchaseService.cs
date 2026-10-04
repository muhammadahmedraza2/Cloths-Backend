using System.Data;
using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class PurchaseService
{
    private readonly AppDbContext _db;
    private readonly NumberGenerator _numbers;

    public PurchaseService(AppDbContext db, NumberGenerator numbers)
    {
        _db = db;
        _numbers = numbers;
    }

    public async Task<Purchase> CreatePurchaseAsync(PurchaseRequestDto dto)
    {
        if (dto.SupplierId == Guid.Empty)
            throw new InvalidOperationException("Supplier is required.");

        if (dto.Items is null || dto.Items.Count == 0)
            throw new InvalidOperationException("Purchase must contain at least one item.");

        await using var transaction =
            await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var supplier = await _db.Suppliers.FirstOrDefaultAsync(x =>
                x.Id == dto.SupplierId && x.IsActive);

            if (supplier is null)
                throw new KeyNotFoundException("Supplier not found.");

            var purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                SupplierId = dto.SupplierId,
                PurchaseNumber = await _numbers.NextAsync("PUR"),
                InvoiceNumber = dto.InvoiceNumber?.Trim(),
                PurchaseDate = dto.PurchaseDate,
                PaymentStatus = dto.PaymentStatus,
                Notes = dto.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.ProductVariantId == Guid.Empty)
                    throw new InvalidOperationException("Product variant is required.");

                if (itemDto.Quantity <= 0)
                    throw new InvalidOperationException("Purchase quantity must be greater than zero.");

                if (itemDto.PurchasePrice < 0)
                    throw new InvalidOperationException("Purchase price cannot be negative.");

                var variant = await _db.ProductVariants
                    .FirstOrDefaultAsync(x => x.Id == itemDto.ProductVariantId);

                if (variant is null)
                    throw new KeyNotFoundException("Product variant not found.");

                variant.StockQuantity += itemDto.Quantity;

                var total = itemDto.PurchasePrice * itemDto.Quantity;

                purchase.Items.Add(new PurchaseItem
                {
                    Id = Guid.NewGuid(),
                    PurchaseId = purchase.Id,
                    ProductVariantId = variant.Id,
                    Quantity = itemDto.Quantity,
                    PurchasePrice = itemDto.PurchasePrice,
                    TotalPrice = total
                });

                _db.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = variant.Id,
                    TransactionType = StockTransactionType.Purchase,
                    Quantity = itemDto.Quantity,
                    ReferenceType = "Purchase",
                    ReferenceId = purchase.Id,
                    Notes = $"Purchase {purchase.PurchaseNumber}",
                    CreatedAt = DateTime.UtcNow
                });
            }

            purchase.TotalAmount = purchase.Items.Sum(x => x.TotalPrice);

            _db.Purchases.Add(purchase);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return purchase;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}