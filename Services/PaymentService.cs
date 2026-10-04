using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class PaymentService
{
    private readonly AppDbContext _db;

    public PaymentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PaymentResponseDto>> GetPaymentsAsync()
    {
        return await _db.Payments
            .AsNoTracking()
            .Include(x => x.Order)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PaymentResponseDto
            {
                Id = x.Id,
                OrderId = x.OrderId,
                OrderNumber = x.Order != null ? x.Order.OrderNumber : "",
                PaymentMethod = x.PaymentMethod,
                Amount = x.Amount,
                PaymentStatus = x.PaymentStatus,
                TransactionReference = x.TransactionReference,
                Provider = x.Provider,
                BankName = x.BankName,
                PaymentProofUrl = x.PaymentProofUrl,
                PaidAt = x.PaidAt
            })
            .ToListAsync();
    }

    public async Task UpdatePaymentStatusAsync(Guid id, PaymentStatus status)
    {
        var payment = await _db.Payments
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (payment is null)
            throw new KeyNotFoundException("Payment not found.");

        payment.PaymentStatus = status;
        payment.PaidAt = status == PaymentStatus.Paid ? DateTime.UtcNow : null;

        if (payment.Order is not null)
        {
            payment.Order.PaymentStatus = status;
            payment.Order.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}