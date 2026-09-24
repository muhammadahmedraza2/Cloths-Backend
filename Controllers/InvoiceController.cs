using System.Security.Claims;
using ClothingErp.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly AppDbContext _db;
    public InvoiceController(AppDbContext db) => _db = db;

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> Get(Guid orderId)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var invoice = await _db.Invoices.AsNoTracking().Include(x => x.Order).ThenInclude(x => x!.Items)
            .Include(x => x.Order).ThenInclude(x => x!.User)
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (invoice?.Order is null || (!isAdmin && invoice.Order.UserId != userId)) return NotFound();

        return Ok(new
        {
            invoice.Id, invoice.InvoiceNumber, invoice.InvoiceDate,
            OrderId = invoice.OrderId, invoice.Order.OrderNumber,
            Customer = invoice.Order.User?.FullName, invoice.Order.FinalAmount,
            PaymentMethod = invoice.Order.PaymentMethod.ToString(),
            PaymentStatus = invoice.Order.PaymentStatus.ToString(),
            Items = invoice.Order.Items.Select(i => new { i.ProductName, i.SKU, i.Quantity, i.UnitPrice, i.TotalPrice })
        });
    }

    [HttpGet("{orderId:guid}/print")]
    public async Task<IActionResult> Print(Guid orderId)
    {
        var isAdmin = User.IsInRole("Admin");
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var invoice = await _db.Invoices.AsNoTracking().Include(x => x.Order).ThenInclude(x => x!.Items).Include(x => x.Order).ThenInclude(x => x!.User).FirstOrDefaultAsync(x => x.OrderId == orderId);
        if (invoice?.Order is null || (!isAdmin && invoice.Order.UserId != userId)) return NotFound();

        var sb = new StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset='utf-8'><title>Invoice ").Append(invoice.InvoiceNumber).Append("</title>");
        sb.Append("<style>body{font-family:Arial;margin:40px}table{width:100%;border-collapse:collapse}th,td{border:1px solid #ccc;padding:8px;text-align:left}.total{text-align:right;font-weight:bold}@media print{button{display:none}}</style></head><body>");
        sb.Append("<button onclick='window.print()'>Print</button>");
        sb.Append("<h1>Invoice</h1><p><b>Invoice No:</b> ").Append(invoice.InvoiceNumber).Append("</p>");
        sb.Append("<p><b>Order No:</b> ").Append(invoice.Order.OrderNumber).Append("</p>");
        sb.Append("<p><b>Customer:</b> ").Append(invoice.Order.User?.FullName).Append("</p><table><thead><tr><th>Product</th><th>SKU</th><th>Qty</th><th>Price</th><th>Total</th></tr></thead><tbody>");
        foreach (var i in invoice.Order.Items) sb.Append("<tr><td>").Append(i.ProductName).Append("</td><td>").Append(i.SKU).Append("</td><td>").Append(i.Quantity).Append("</td><td>").Append(i.UnitPrice.ToString("N2")).Append("</td><td>").Append(i.TotalPrice.ToString("N2")).Append("</td></tr>");
        sb.Append("</tbody></table><p class='total'>Final Amount: ").Append(invoice.Order.FinalAmount.ToString("N2")).Append("</p></body></html>");
        return Content(sb.ToString(), "text/html; charset=utf-8");
    }
}
