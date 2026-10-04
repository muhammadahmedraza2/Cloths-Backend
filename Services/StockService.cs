using ClothingErp.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class StockService
{
    private readonly AppDbContext _db;

    public StockService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<object>> GetStockHistoryAsync(Guid variantId) =>
        _db.StockTransactions
            .AsNoTracking()
            .Where(x => x.ProductVariantId == variantId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => (object)new
            {
                x.Id,
                x.ProductVariantId,
                x.TransactionType,
                x.Quantity,
                x.ReferenceType,
                x.ReferenceId,
                x.Notes,
                x.CreatedAt,
                x.CreatedBy
            })
            .ToListAsync();
}