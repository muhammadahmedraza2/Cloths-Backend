using ClothingErp.Api.Models;

namespace ClothingErp.Api.Services;

public record PaymentGatewayResult(bool Success, string Status, string? TransactionReference, string? Message);

public interface IPaymentGateway
{
    Task<PaymentGatewayResult> ChargeAsync(decimal amount, string reference, CancellationToken cancellationToken = default);
}

public class PaymentGatewayService : IPaymentGateway
{
    private readonly IConfiguration _configuration;

    public PaymentGatewayService(IConfiguration configuration) => _configuration = configuration;

    public Task<PaymentGatewayResult> ChargeAsync(decimal amount, string reference, CancellationToken cancellationToken = default)
    {
        // Test/mock gateway. No card number, CVV or PIN is accepted or stored by this API.
        var enabled = _configuration.GetValue<bool>("PaymentSettings:MockSuccess");
        return Task.FromResult(enabled
            ? new PaymentGatewayResult(true, nameof(PaymentStatus.Paid), $"TEST-{Guid.NewGuid():N}", "Mock payment successful.")
            : new PaymentGatewayResult(false, nameof(PaymentStatus.Failed), null, "Payment gateway is not configured for live processing."));
    }
}
