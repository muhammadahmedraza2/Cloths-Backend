using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class DashboardResponseDto
{
    public int TotalUsers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public decimal TodaysSales { get; set; }
    public decimal MonthlySales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalPurchases { get; set; }
    public int LowStockProducts { get; set; }
    public int PendingPayments { get; set; }
}
