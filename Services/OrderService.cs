using System.Data;
using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class OrderService
{
    private readonly AppDbContext _db;
    private readonly IPaymentGateway _gateway;
    private readonly NumberGenerator _numbers;

    public OrderService(
        AppDbContext db,
        IPaymentGateway gateway,
        NumberGenerator numbers)
    {
        _db = db;
        _gateway = gateway;
        _numbers = numbers;
    }

    public async Task<OrderResponseDto> CreateOrderAsync(
        Guid userId,
        CreateOrderRequestDto dto)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var address = await _db.Addresses.FirstOrDefaultAsync(x =>
                x.Id == dto.ShippingAddressId && x.UserId == userId);

            if (address is null)
                throw new KeyNotFoundException("Shipping address not found.");

            var cart = await _db.Carts.FirstOrDefaultAsync(x => x.UserId == userId);

            if (cart is null)
                throw new InvalidOperationException("Cart is empty.");

            var items = await _db.CartItems
                .Include(x => x.ProductVariant)
                    .ThenInclude(x => x!.Product)
                .Where(x => x.CartId == cart.Id)
                .ToListAsync();

            if (items.Count == 0)
                throw new InvalidOperationException("Your cart is empty.");

            foreach (var item in items)
            {
                if (item.ProductVariant is null ||
                    item.ProductVariant.Product is null ||
                    !item.ProductVariant.IsActive ||
                    !item.ProductVariant.Product.IsActive)
                {
                    throw new InvalidOperationException(
                        "One or more products are inactive.");
                }

                if (item.ProductVariant.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for {item.ProductVariant.Product.ProductName}.");
                }
            }

            var subtotal = items.Sum(x => x.ProductVariant!.SalePrice * x.Quantity);

            var shippingAmount = dto.ShippingAmount < 0 ? 0 : dto.ShippingAmount;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderNumber = await _numbers.NextAsync("ORD"),
                TotalAmount = subtotal,
                DiscountAmount = 0,
                ShippingAmount = shippingAmount,
                FinalAmount = subtotal + shippingAmount,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                OrderStatus = OrderStatus.Pending,
                ShippingAddressId = address.Id,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var cartItem in items)
            {
                var variant = cartItem.ProductVariant!;

                variant.StockQuantity -= cartItem.Quantity;

                _db.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = variant.Id,
                    TransactionType = StockTransactionType.Sale,
                    Quantity = cartItem.Quantity,
                    ReferenceType = "Order",
                    ReferenceId = order.Id,
                    Notes = $"Order {order.OrderNumber}",
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow
                });

                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductVariantId = variant.Id,
                    ProductName = variant.Product!.ProductName,
                    SKU = variant.SKU,
                    Quantity = cartItem.Quantity,
                    UnitPrice = variant.SalePrice,
                    TotalPrice = variant.SalePrice * cartItem.Quantity
                });
            }

            order.Invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                InvoiceNumber = await _numbers.NextAsync("INV"),
                InvoiceDate = DateTime.UtcNow,
                Status = "Issued",
                CreatedAt = DateTime.UtcNow
            };

            if (dto.PaymentMethod == PaymentMethod.OnlineBankTransfer)
            {
                if (!dto.PaymentProofId.HasValue)
                {
                    throw new InvalidOperationException(
                        "Payment proof is required for online bank transfer.");
                }

                var proof = await _db.PaymentProofs.FirstOrDefaultAsync(x =>
                    x.Id == dto.PaymentProofId.Value && x.UserId == userId);

                if (proof is null)
                {
                    throw new InvalidOperationException(
                        "Payment proof was not found or does not belong to the current user.");
                }
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                PaymentMethod = dto.PaymentMethod,
                Amount = order.FinalAmount,
                PaymentStatus = PaymentStatus.Pending,
                BankName = dto.BankName,
                TransactionReference = dto.TransactionReference,
                PaymentProofUrl = dto.PaymentProofId.HasValue
                    ? $"/api/payment-proof/{dto.PaymentProofId.Value}"
                    : dto.PaymentProofUrl,
                Provider = dto.PaymentMethod == PaymentMethod.CashOnDelivery
                    ? "Cash"
                    : "Manual",
                CreatedAt = DateTime.UtcNow
            };

            if (dto.PaymentMethod == PaymentMethod.Card)
            {
                var gatewayResult = await _gateway.ChargeAsync(
                    order.FinalAmount,
                    order.OrderNumber);

                payment.PaymentStatus = gatewayResult.Success
                    ? PaymentStatus.Paid
                    : PaymentStatus.Failed;

                payment.Provider = "MockGateway";
                payment.TransactionReference = gatewayResult.TransactionReference;

                order.PaymentStatus = payment.PaymentStatus;

                if (payment.PaymentStatus == PaymentStatus.Paid)
                    payment.PaidAt = DateTime.UtcNow;
            }

            _db.Orders.Add(order);
            _db.Payments.Add(payment);
            _db.CartItems.RemoveRange(items);

            cart.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return (await GetOrderAsync(userId, order.Id, false))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrderResponseDto>> GetMyOrdersAsync(Guid userId)
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Invoice)
            .Include(x => x.Items)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return orders.Select(MapOrder).ToList();
    }

    public async Task<OrderResponseDto?> GetOrderAsync(
        Guid userId,
        Guid id,
        bool admin)
    {
        var query = _db.Orders
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Invoice)
            .Include(x => x.Items)
            .AsQueryable();

        if (!admin)
            query = query.Where(x => x.UserId == userId);

        var order = await query.FirstOrDefaultAsync(x => x.Id == id);

        return order is null ? null : MapOrder(order);
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Invoice)
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return orders.Select(MapOrder).ToList();
    }

    public async Task UpdateOrderStatusAsync(Guid id, OrderStatus status)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(x => x.Id == id);

        if (order is null)
            throw new KeyNotFoundException("Order not found.");

        if (order.OrderStatus == status)
            return;

        if (status == OrderStatus.Cancelled &&
            order.OrderStatus != OrderStatus.Cancelled)
        {
            var items = await _db.OrderItems
                .Where(x => x.OrderId == id)
                .ToListAsync();

            foreach (var item in items)
            {
                var variant = await _db.ProductVariants
                    .FirstOrDefaultAsync(x => x.Id == item.ProductVariantId);

                if (variant is null)
                    continue;

                variant.StockQuantity += item.Quantity;

                _db.StockTransactions.Add(new StockTransaction
                {
                    Id = Guid.NewGuid(),
                    ProductVariantId = variant.Id,
                    TransactionType = StockTransactionType.Return,
                    Quantity = item.Quantity,
                    ReferenceType = "OrderCancel",
                    ReferenceId = id,
                    Notes = "Stock restored after order cancellation.",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        order.OrderStatus = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static OrderResponseDto MapOrder(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            InvoiceNumber = order.Invoice?.InvoiceNumber,
            UserId = order.UserId,
            CustomerName = order.User?.FullName,
            TotalAmount = order.TotalAmount,
            DiscountAmount = order.DiscountAmount,
            ShippingAmount = order.ShippingAmount,
            FinalAmount = order.FinalAmount,
            PaymentMethod = order.PaymentMethod,
            PaymentStatus = order.PaymentStatus,
            OrderStatus = order.OrderStatus,
            CreatedAt = order.CreatedAt,
            Items = order.Items
                .Select(i => new OrderItemResponseDto
                {
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductName,
                    SKU = i.SKU,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                })
                .ToList()
        };
    }
}