using System.Data;
using System.Security.Claims;
using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class EcommerceService
{
    private readonly AppDbContext _db;
    private readonly IPaymentGateway _gateway;

    public EcommerceService(AppDbContext db, IPaymentGateway gateway)
    {
        _db = db;
        _gateway = gateway;
    }

    public async Task<List<ProductResponseDto>> GetProductsAsync(string? search, Guid? categoryId, Guid? ageGroupId, Guid? sizeId, Guid? colorId)
    {
        var q = _db.Products.AsNoTracking()
            .Include(x => x.Category).Include(x => x.Brand).Include(x => x.AgeGroup)
            .Include(x => x.Images).Include(x => x.Variants).ThenInclude(v => v.Size)
            .Include(x => x.Variants).ThenInclude(v => v.Color)
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.ProductName.Contains(search) || x.SKU.Contains(search));
        if (categoryId.HasValue) q = q.Where(x => x.CategoryId == categoryId);
        if (ageGroupId.HasValue) q = q.Where(x => x.AgeGroupId == ageGroupId);
        if (sizeId.HasValue) q = q.Where(x => x.Variants.Any(v => v.SizeId == sizeId && v.IsActive));
        if (colorId.HasValue) q = q.Where(x => x.Variants.Any(v => v.ColorId == colorId && v.IsActive));

        return await q.OrderByDescending(x => x.CreatedAt).Select(ToProductResponse()).ToListAsync();
    }

    public async Task<ProductResponseDto?> GetProductAsync(Guid id)
    {
        var p = await _db.Products.AsNoTracking()
            .Include(x => x.Category).Include(x => x.Brand).Include(x => x.AgeGroup)
            .Include(x => x.Images).Include(x => x.Variants).ThenInclude(v => v.Size)
            .Include(x => x.Variants).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? null : MapProduct(p);
    }

    public async Task<ProductResponseDto> SaveProductAsync(Guid? id, ProductRequestDto dto)
    {
        if (!await _db.Categories.AnyAsync(x => x.Id == dto.CategoryId && x.IsActive))
            throw new InvalidOperationException("Category does not exist or is inactive.");

        if (dto.SalePrice < 0 || dto.PurchasePrice < 0 || dto.Discount < 0)
            throw new InvalidOperationException("Prices cannot be negative.");

        Product p;
        if (id.HasValue)
        {
            p = await _db.Products.Include(x => x.Variants).Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new KeyNotFoundException("Product not found.");

            p.ProductName = dto.ProductName.Trim(); p.SKU = dto.SKU.Trim(); p.Description = dto.Description;
            p.CategoryId = dto.CategoryId; p.BrandId = dto.BrandId; p.Gender = dto.Gender; p.AgeGroupId = dto.AgeGroupId;
            p.Fabric = dto.Fabric; p.Season = dto.Season; p.PurchasePrice = dto.PurchasePrice; p.SalePrice = dto.SalePrice;
            p.Discount = dto.Discount; p.MinimumStockLevel = dto.MinimumStockLevel; p.IsActive = dto.IsActive; p.UpdatedAt = DateTime.UtcNow;
            _db.ProductImages.RemoveRange(p.Images);
            _db.ProductVariants.RemoveRange(p.Variants);
        }
        else
        {
            p = new Product
            {
                ProductName = dto.ProductName.Trim(), SKU = dto.SKU.Trim(), Description = dto.Description,
                CategoryId = dto.CategoryId, BrandId = dto.BrandId, Gender = dto.Gender, AgeGroupId = dto.AgeGroupId,
                Fabric = dto.Fabric, Season = dto.Season, PurchasePrice = dto.PurchasePrice, SalePrice = dto.SalePrice,
                Discount = dto.Discount, MinimumStockLevel = dto.MinimumStockLevel, IsActive = dto.IsActive
            };
            await _db.Products.AddAsync(p);
        }

        p.Variants = dto.Variants.Select(v => new ProductVariant
        {
            ProductId = p.Id, SizeId = v.SizeId, ColorId = v.ColorId, SKU = v.SKU.Trim(),
            PurchasePrice = v.PurchasePrice, SalePrice = v.SalePrice, StockQuantity = v.StockQuantity,
            MinimumStockLevel = v.MinimumStockLevel, IsActive = v.IsActive
        }).ToList();

        p.Images = dto.ImageUrls.Where(x => !string.IsNullOrWhiteSpace(x)).Select((x, i) => new ProductImage
        {
            ProductId = p.Id, ImageUrl = x.Trim(), IsPrimary = i == 0
        }).ToList();

        p.StockQuantity = p.Variants.Sum(x => x.StockQuantity);
        await _db.SaveChangesAsync();
        return (await GetProductAsync(p.Id))!;
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var p = await _db.Products.FindAsync(id) ?? throw new KeyNotFoundException("Product not found.");
        p.IsActive = false;
        p.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<CartResponseDto> GetCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await MapCartAsync(cart.Id);
    }

    public async Task<CartResponseDto> AddToCartAsync(Guid userId, AddToCartRequestDto dto)
    {
        if (dto.Quantity <= 0) throw new InvalidOperationException("Quantity must be greater than zero.");
        var variant = await _db.ProductVariants.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == dto.ProductVariantId && x.IsActive);
        if (variant?.Product is null || !variant.Product.IsActive) throw new KeyNotFoundException("Product variant not found or inactive.");
        if (variant.StockQuantity < dto.Quantity) throw new InvalidOperationException("Insufficient stock.");

        var cart = await GetOrCreateCartAsync(userId);
        var item = await _db.CartItems.FirstOrDefaultAsync(x => x.CartId == cart.Id && x.ProductVariantId == dto.ProductVariantId);
        if (item is null)
        {
            item = new CartItem { CartId = cart.Id, UserId = userId, ProductVariantId = dto.ProductVariantId, Quantity = dto.Quantity, UnitPrice = variant.SalePrice, ProductId = variant.ProductId.ToString(), Name = variant.Product.ProductName };
            await _db.CartItems.AddAsync(item);
        }
        else
        {
            if (variant.StockQuantity < item.Quantity + dto.Quantity) throw new InvalidOperationException("Insufficient stock.");
            item.Quantity += dto.Quantity;
            item.UnitPrice = variant.SalePrice;
        }
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await MapCartAsync(cart.Id);
    }

    public async Task<CartResponseDto> UpdateCartAsync(Guid userId, Guid itemId, int quantity)
    {
        if (quantity <= 0) throw new InvalidOperationException("Quantity must be greater than zero.");
        var item = await _db.CartItems.Include(x => x.ProductVariant).FirstOrDefaultAsync(x => x.Id == itemId && x.UserId == userId)
            ?? throw new KeyNotFoundException("Cart item not found.");
        if (item.ProductVariant is null || item.ProductVariant.StockQuantity < quantity) throw new InvalidOperationException("Insufficient stock.");
        item.Quantity = quantity; item.UnitPrice = item.ProductVariant.SalePrice;
        await _db.SaveChangesAsync();
        return await MapCartAsync(item.CartId);
    }

    public async Task<CartResponseDto> RemoveCartItemAsync(Guid userId, Guid itemId)
    {
        var item = await _db.CartItems.FirstOrDefaultAsync(x => x.Id == itemId && x.UserId == userId)
            ?? throw new KeyNotFoundException("Cart item not found.");
        var cartId = item.CartId;
        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return await MapCartAsync(cartId);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var items = await _db.CartItems.Where(x => x.UserId == userId).ToListAsync();
        _db.CartItems.RemoveRange(items);
        await _db.SaveChangesAsync();
    }

    public async Task<OrderResponseDto> CreateOrderAsync(Guid userId, CreateOrderRequestDto dto)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var address = await _db.Addresses.FirstOrDefaultAsync(x => x.Id == dto.ShippingAddressId && x.UserId == userId)
            ?? throw new KeyNotFoundException("Shipping address not found.");

        var cart = await _db.Carts.FirstOrDefaultAsync(x => x.UserId == userId) ?? throw new InvalidOperationException("Cart is empty.");
        var items = await _db.CartItems.Include(x => x.ProductVariant).ThenInclude(v => v!.Product).Where(x => x.CartId == cart.Id).ToListAsync();
        if (items.Count == 0) throw new InvalidOperationException("Your cart is empty.");

        foreach (var item in items)
        {
            if (item.ProductVariant is null || item.ProductVariant.Product is null || !item.ProductVariant.IsActive || !item.ProductVariant.Product.IsActive)
                throw new InvalidOperationException("One or more products are inactive.");
            if (item.ProductVariant.StockQuantity < item.Quantity) throw new InvalidOperationException($"Insufficient stock for {item.ProductVariant.Product.ProductName}.");
        }

        var subtotal = items.Sum(x => x.ProductVariant!.SalePrice * x.Quantity);
        var order = new Order
        {
            UserId = userId, OrderNumber = await NextNumberAsync("ORD"),
            TotalAmount = subtotal, DiscountAmount = 0, ShippingAmount = dto.ShippingAmount < 0 ? 0 : dto.ShippingAmount,
            FinalAmount = subtotal + (dto.ShippingAmount < 0 ? 0 : dto.ShippingAmount),
            PaymentMethod = dto.PaymentMethod, PaymentStatus = dto.PaymentMethod == PaymentMethod.CashOnDelivery ? PaymentStatus.Pending : PaymentStatus.Pending,
            OrderStatus = OrderStatus.Pending, ShippingAddressId = address.Id
        };

        foreach (var item in items)
        {
            var variant = item.ProductVariant!;
            variant.StockQuantity -= item.Quantity;
            _db.StockTransactions.Add(new StockTransaction
            {
                ProductVariantId = variant.Id, TransactionType = StockTransactionType.Sale, Quantity = item.Quantity,
                ReferenceType = "Order", ReferenceId = order.Id, Notes = $"Order {order.OrderNumber}", CreatedBy = userId
            });
            order.Items.Add(new OrderItem
            {
                ProductVariantId = variant.Id, ProductName = variant.Product!.ProductName, SKU = variant.SKU,
                Quantity = item.Quantity, UnitPrice = variant.SalePrice, TotalPrice = variant.SalePrice * item.Quantity
            });
        }

        order.Invoice = new Invoice { OrderId = order.Id, InvoiceNumber = await NextNumberAsync("INV") };
        _db.Orders.Add(order);

        Guid? paymentProofId = dto.PaymentProofId;
        if (dto.PaymentMethod == PaymentMethod.OnlineBankTransfer)
        {
            if (!paymentProofId.HasValue)
                throw new InvalidOperationException("Payment proof is required for online bank transfer.");

            var proof = await _db.PaymentProofs.FirstOrDefaultAsync(x => x.Id == paymentProofId.Value && x.UserId == userId);
            if (proof is null)
                throw new InvalidOperationException("Payment proof was not found or does not belong to the current user.");
        }

        var payment = new Payment
        {
            OrderId = order.Id, PaymentMethod = dto.PaymentMethod, Amount = order.FinalAmount,
            PaymentStatus = PaymentStatus.Pending, BankName = dto.BankName, TransactionReference = dto.TransactionReference,
            PaymentProofUrl = paymentProofId.HasValue ? $"/api/payment-proof/{paymentProofId.Value}" : dto.PaymentProofUrl,
            Provider = dto.PaymentMethod == PaymentMethod.CashOnDelivery ? "Cash" : "Manual"
        };

        if (dto.PaymentMethod == PaymentMethod.Card)
        {
            var result = await _gateway.ChargeAsync(order.FinalAmount, order.OrderNumber);
            payment.PaymentStatus = result.Success ? PaymentStatus.Paid : PaymentStatus.Failed;
            payment.Provider = "MockGateway";
            payment.TransactionReference = result.TransactionReference;
            order.PaymentStatus = payment.PaymentStatus;
        }
        else if (dto.PaymentMethod == PaymentMethod.OnlineBankTransfer)
        {
            payment.PaymentStatus = PaymentStatus.Pending;
            order.PaymentStatus = PaymentStatus.Pending;
        }

        _db.Payments.Add(payment);
        _db.CartItems.RemoveRange(items);
        cart.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return (await GetOrderAsync(userId, order.Id, false))!;
    }

    public async Task<List<OrderResponseDto>> GetMyOrdersAsync(Guid userId)
    {
        var orders = await _db.Orders.AsNoTracking().Include(x => x.User).Include(x => x.Invoice).Include(x => x.Items)
            .Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToListAsync();
        return orders.Select(MapOrder).ToList();
    }

    public async Task<OrderResponseDto?> GetOrderAsync(Guid userId, Guid id, bool admin)
    {
        var q = _db.Orders.AsNoTracking().Include(x => x.User).Include(x => x.Invoice).Include(x => x.Items).AsQueryable();
        if (!admin) q = q.Where(x => x.UserId == userId);
        var order = await q.FirstOrDefaultAsync(x => x.Id == id);
        return order is null ? null : MapOrder(order);
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _db.Orders.AsNoTracking().Include(x => x.User).Include(x => x.Invoice).Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();
        return orders.Select(MapOrder).ToList();
    }

    public async Task UpdateOrderStatusAsync(Guid id, OrderStatus status)
    {
        var order = await _db.Orders.FindAsync(id) ?? throw new KeyNotFoundException("Order not found.");
        order.OrderStatus = status; order.UpdatedAt = DateTime.UtcNow;
        if (status == OrderStatus.Cancelled)
        {
            var items = await _db.OrderItems.Where(x => x.OrderId == id).ToListAsync();
            foreach (var item in items)
            {
                var variant = await _db.ProductVariants.FindAsync(item.ProductVariantId);
                if (variant is not null)
                {
                    variant.StockQuantity += item.Quantity;
                    _db.StockTransactions.Add(new StockTransaction { ProductVariantId = variant.Id, TransactionType = StockTransactionType.Return, Quantity = item.Quantity, ReferenceType = "OrderCancel", ReferenceId = id, Notes = "Stock restored after order cancellation." });
                }
            }
        }
        await _db.SaveChangesAsync();
    }

    public async Task<List<PaymentResponseDto>> GetPaymentsAsync()
    {
        return await _db.Payments.AsNoTracking().Include(x => x.Order).OrderByDescending(x => x.CreatedAt)
            .Select(x => new PaymentResponseDto { Id = x.Id, OrderId = x.OrderId, OrderNumber = x.Order!.OrderNumber, PaymentMethod = x.PaymentMethod, Amount = x.Amount, PaymentStatus = x.PaymentStatus, TransactionReference = x.TransactionReference, Provider = x.Provider, BankName = x.BankName, PaymentProofUrl = x.PaymentProofUrl, PaidAt = x.PaidAt })
            .ToListAsync();
    }

    public async Task UpdatePaymentStatusAsync(Guid id, PaymentStatus status)
    {
        var payment = await _db.Payments.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == id) ?? throw new KeyNotFoundException("Payment not found.");
        payment.PaymentStatus = status;
        payment.PaidAt = status == PaymentStatus.Paid ? DateTime.UtcNow : payment.PaidAt;
        if (payment.Order is not null) payment.Order.PaymentStatus = status;
        await _db.SaveChangesAsync();
    }

    public async Task<List<Address>> GetAddressesAsync(Guid userId) => await _db.Addresses.AsNoTracking().Where(x => x.UserId == userId).OrderByDescending(x => x.IsDefault).ToListAsync();

    public async Task<Address> AddAddressAsync(Guid userId, AddressRequestDto dto)
    {
        if (dto.IsDefault) await _db.Addresses.Where(x => x.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDefault, false));
        var address = new Address { UserId = userId, AddressLine = dto.AddressLine.Trim(), City = dto.City.Trim(), Area = dto.Area?.Trim(), PostalCode = dto.PostalCode?.Trim(), Country = dto.Country.Trim(), IsDefault = dto.IsDefault };
        if (!await _db.Addresses.AnyAsync(x => x.UserId == userId)) address.IsDefault = true;
        _db.Addresses.Add(address); await _db.SaveChangesAsync(); return address;
    }

    public async Task DeleteAddressAsync(Guid userId, Guid id)
    {
        var a = await _db.Addresses.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId) ?? throw new KeyNotFoundException("Address not found.");
        _db.Addresses.Remove(a); await _db.SaveChangesAsync();
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
            PendingOrders = await _db.Orders.CountAsync(x => x.OrderStatus == OrderStatus.Pending),
            TodaysSales = await _db.Orders.Where(x => x.CreatedAt >= today && x.PaymentStatus == PaymentStatus.Paid).SumAsync(x => (decimal?)x.FinalAmount) ?? 0,
            MonthlySales = await _db.Orders.Where(x => x.CreatedAt >= month && x.PaymentStatus == PaymentStatus.Paid).SumAsync(x => (decimal?)x.FinalAmount) ?? 0,
            TotalRevenue = await _db.Orders.Where(x => x.PaymentStatus == PaymentStatus.Paid).SumAsync(x => (decimal?)x.FinalAmount) ?? 0,
            TotalPurchases = await _db.Purchases.SumAsync(x => (decimal?)x.TotalAmount) ?? 0,
            LowStockProducts = await _db.ProductVariants.CountAsync(x => x.IsActive && x.StockQuantity <= x.MinimumStockLevel),
            PendingPayments = await _db.Payments.CountAsync(x => x.PaymentStatus == PaymentStatus.Pending)
        };
    }

    public async Task<List<UserResponseDto>> GetUsersAsync() =>
        await _db.Users.AsNoTracking().Select(x => new UserResponseDto { Id = x.Id, Username = x.UserName!, Email = x.Email, PhoneNumber = x.PhoneNumber, FullName = x.FullName, Role = x.Role, IsActive = x.IsActive }).ToListAsync();

    public async Task SetUserActiveAsync(Guid id, bool active)
    {
        var user = await _db.Users.FindAsync(id) ?? throw new KeyNotFoundException("User not found.");
        user.IsActive = active; await _db.SaveChangesAsync();
    }

    public async Task<Purchase> CreatePurchaseAsync(PurchaseRequestDto dto)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (!await _db.Suppliers.AnyAsync(x => x.Id == dto.SupplierId && x.IsActive)) throw new KeyNotFoundException("Supplier not found.");

        var purchase = new Purchase { SupplierId = dto.SupplierId, PurchaseNumber = await NextNumberAsync("PUR"), InvoiceNumber = dto.InvoiceNumber, PurchaseDate = dto.PurchaseDate, PaymentStatus = dto.PaymentStatus, Notes = dto.Notes };
        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0 || item.PurchasePrice < 0) throw new InvalidOperationException("Invalid purchase item.");
            var variant = await _db.ProductVariants.FindAsync(item.ProductVariantId) ?? throw new KeyNotFoundException("Product variant not found.");
            variant.StockQuantity += item.Quantity;
            purchase.Items.Add(new PurchaseItem { ProductVariantId = variant.Id, Quantity = item.Quantity, PurchasePrice = item.PurchasePrice, TotalPrice = item.PurchasePrice * item.Quantity });
            _db.StockTransactions.Add(new StockTransaction { ProductVariantId = variant.Id, TransactionType = StockTransactionType.Purchase, Quantity = item.Quantity, ReferenceType = "Purchase", ReferenceId = purchase.Id, Notes = $"Purchase {purchase.PurchaseNumber}" });
        }
        purchase.TotalAmount = purchase.Items.Sum(x => x.TotalPrice);
        _db.Purchases.Add(purchase); await _db.SaveChangesAsync(); await tx.CommitAsync(); return purchase;
    }

    public async Task<List<Supplier>> GetSuppliersAsync() => await _db.Suppliers.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

    public async Task<Supplier> SaveSupplierAsync(Guid? id, SupplierRequestDto dto)
    {
        Supplier s;
        if (id.HasValue) s = await _db.Suppliers.FindAsync(id) ?? throw new KeyNotFoundException("Supplier not found.");
        else { s = new Supplier(); _db.Suppliers.Add(s); }
        s.Name = dto.Name.Trim(); s.Phone = dto.Phone?.Trim(); s.Email = dto.Email?.Trim(); s.Address = dto.Address?.Trim(); s.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(); return s;
    }

    public async Task<List<object>> GetStockHistoryAsync(Guid variantId) =>
        await _db.StockTransactions.AsNoTracking().Where(x => x.ProductVariantId == variantId).OrderByDescending(x => x.CreatedAt)
            .Select(x => (object)new { x.Id, x.ProductVariantId, x.TransactionType, x.Quantity, x.ReferenceType, x.ReferenceId, x.Notes, x.CreatedAt, x.CreatedBy }).ToListAsync();

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _db.Carts.FirstOrDefaultAsync(x => x.UserId == userId);
        if (cart is null) { cart = new Cart { UserId = userId }; _db.Carts.Add(cart); await _db.SaveChangesAsync(); }
        return cart;
    }

    private async Task<CartResponseDto> MapCartAsync(Guid cartId)
    {
        var cart = await _db.Carts.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.ProductVariant).ThenInclude(v => v!.Product)
            .Include(x => x.Items).ThenInclude(x => x.ProductVariant).ThenInclude(v => v!.Size)
            .Include(x => x.Items).ThenInclude(x => x.ProductVariant).ThenInclude(v => v!.Color)
            .Include(x => x.Items).ThenInclude(x => x.ProductVariant).ThenInclude(v => v!.Product).ThenInclude(p => p!.Images)
            .FirstAsync(x => x.Id == cartId);

        var items = cart.Items.Select(x => new CartItemResponseDto
        {
            Id = x.Id, ProductVariantId = x.ProductVariantId, ProductName = x.ProductVariant!.Product!.ProductName, SKU = x.ProductVariant.SKU,
            ImageUrl = x.ProductVariant.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? x.ProductVariant.Product.Images.FirstOrDefault()?.ImageUrl,
            Size = x.ProductVariant.Size!.Name, Color = x.ProductVariant.Color!.Name, UnitPrice = x.ProductVariant.SalePrice,
            Quantity = x.Quantity, Total = x.ProductVariant.SalePrice * x.Quantity
        }).ToList();

        return new CartResponseDto { CartId = cart.Id, Items = items, TotalQuantity = items.Sum(x => x.Quantity), TotalAmount = items.Sum(x => x.Total) };
    }

    private static ProductResponseDto MapProduct(Product p) => new()
    {
        Id = p.Id, ProductName = p.ProductName, SKU = p.SKU, Description = p.Description, CategoryId = p.CategoryId, CategoryName = p.Category?.Name,
        BrandId = p.BrandId, BrandName = p.Brand?.Name, Gender = p.Gender, AgeGroupId = p.AgeGroupId, AgeGroupName = p.AgeGroup?.Name,
        Fabric = p.Fabric, Season = p.Season, PurchasePrice = p.PurchasePrice, SalePrice = p.SalePrice, Discount = p.Discount,
        StockQuantity = p.Variants.Sum(v => v.StockQuantity), MinimumStockLevel = p.MinimumStockLevel, IsActive = p.IsActive,
        Images = p.Images.Select(i => i.ImageUrl).ToList(),
        Variants = p.Variants.Select(v => new ProductVariantResponseDto { Id = v.Id, SizeId = v.SizeId, SizeName = v.Size?.Name ?? "", ColorId = v.ColorId, ColorName = v.Color?.Name ?? "", SKU = v.SKU, PurchasePrice = v.PurchasePrice, SalePrice = v.SalePrice, StockQuantity = v.StockQuantity, MinimumStockLevel = v.MinimumStockLevel, IsActive = v.IsActive }).ToList()
    };

    private static ProductResponseDto MapProductNoOp(Product p) => MapProduct(p);
    private static System.Linq.Expressions.Expression<Func<Product, ProductResponseDto>> ToProductResponse() => p => new ProductResponseDto
    {
        Id = p.Id, ProductName = p.ProductName, SKU = p.SKU, Description = p.Description, CategoryId = p.CategoryId, CategoryName = p.Category!.Name,
        BrandId = p.BrandId, BrandName = p.Brand != null ? p.Brand.Name : null, Gender = p.Gender, AgeGroupId = p.AgeGroupId, AgeGroupName = p.AgeGroup != null ? p.AgeGroup.Name : null,
        Fabric = p.Fabric, Season = p.Season, PurchasePrice = p.PurchasePrice, SalePrice = p.SalePrice, Discount = p.Discount,
        StockQuantity = p.Variants.Sum(v => v.StockQuantity), MinimumStockLevel = p.MinimumStockLevel, IsActive = p.IsActive,
        Images = p.Images.Select(i => i.ImageUrl).ToList(),
        Variants = p.Variants.Select(v => new ProductVariantResponseDto { Id = v.Id, SizeId = v.SizeId, SizeName = v.Size!.Name, ColorId = v.ColorId, ColorName = v.Color!.Name, SKU = v.SKU, PurchasePrice = v.PurchasePrice, SalePrice = v.SalePrice, StockQuantity = v.StockQuantity, MinimumStockLevel = v.MinimumStockLevel, IsActive = v.IsActive }).ToList()
    };

    private static OrderResponseDto MapOrder(Order o) => new()
    {
        Id = o.Id, OrderNumber = o.OrderNumber, InvoiceNumber = o.Invoice?.InvoiceNumber, UserId = o.UserId, CustomerName = o.User?.FullName,
        TotalAmount = o.TotalAmount, DiscountAmount = o.DiscountAmount, ShippingAmount = o.ShippingAmount, FinalAmount = o.FinalAmount,
        PaymentMethod = o.PaymentMethod, PaymentStatus = o.PaymentStatus, OrderStatus = o.OrderStatus, CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new OrderItemResponseDto { ProductVariantId = i.ProductVariantId, ProductName = i.ProductName, SKU = i.SKU, Quantity = i.Quantity, UnitPrice = i.UnitPrice, TotalPrice = i.TotalPrice }).ToList()
    };

    private async Task<string> NextNumberAsync(string prefix)
    {
        var now = DateTime.UtcNow;
        var date = now.ToString("yyyyMMdd");
        var count = prefix switch
        {
            "ORD" => await _db.Orders.CountAsync(x => x.CreatedAt.Date == now.Date) + 1,
            "INV" => await _db.Invoices.CountAsync(x => x.CreatedAt.Date == now.Date) + 1,
            "PUR" => await _db.Purchases.CountAsync(x => x.CreatedAt.Date == now.Date) + 1,
            _ => 1
        };
        return $"{prefix}-{date}-{count:0000}";
    }
}
