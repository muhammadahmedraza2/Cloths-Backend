using ClothingErp.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class NumberGenerator
{
    private readonly AppDbContext _db;

    public NumberGenerator(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> NextAsync(string prefix)
    {
        var now = DateTime.UtcNow;
        var date = now.ToString("yyyyMMdd");

        int count;

        switch (prefix)
        {
            case "ORD":
                count = await _db.Orders
                    .CountAsync(x => x.CreatedAt.Date == now.Date) + 1;
                break;

            case "INV":
                count = await _db.Invoices
                    .CountAsync(x => x.CreatedAt.Date == now.Date) + 1;
                break;

            case "PUR":
                count = await _db.Purchases
                    .CountAsync(x => x.CreatedAt.Date == now.Date) + 1;
                break;

            default:
                count = 1;
                break;
        }

        return $"{prefix}-{date}-{count:0000}";
    }
}