using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class DashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardResponseDto> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var month = new DateTime(today.Year, today.Month, 1);

        return new DashboardResponseDto
        {
            TotalUsers = await _db.Users.CountAsync(x => x.Role == "User"),

            TotalProducts = await _db.Products.CountAsync(x => x.IsActive),

            TotalOrders = await _db.Orders.CountAsync(),

            PendingOrders = await _db.Orders
                .CountAsync(x => x.OrderStatus == OrderStatus.Pending),

            TodaysSales = await _db.Orders
                .Where(x => x.CreatedAt >= today &&
                            x.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(x => (decimal?)x.FinalAmount) ?? 0,

            MonthlySales = await _db.Orders
                .Where(x => x.CreatedAt >= month &&
                            x.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(x => (decimal?)x.FinalAmount) ?? 0,

            TotalRevenue = await _db.Orders
                .Where(x => x.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(x => (decimal?)x.FinalAmount) ?? 0,

            TotalPurchases = await _db.Purchases
                .SumAsync(x => (decimal?)x.TotalAmount) ?? 0,

            LowStockProducts = await _db.ProductVariants
                .CountAsync(x => x.IsActive &&
                                 x.StockQuantity <= x.MinimumStockLevel),

            PendingPayments = await _db.Payments
                .CountAsync(x => x.PaymentStatus == PaymentStatus.Pending)
        };
    }
}