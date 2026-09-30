using System.Data;
using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class EcommerceService
{
    private readonly AppDbContext _db;
    private readonly IPaymentGateway _gateway;

    public EcommerceService(
        AppDbContext db,
        IPaymentGateway gateway)
    {
        _db = db;
        _gateway = gateway;
    }

    // =========================================================
    // CATALOG
    // =========================================================

    public async Task<CatalogProductsResultDto> GetProductsAsync(
        string? search,
        Guid? departmentId,
        Guid? categoryId,
        Gender? gender,
        Guid? ageGroupId,
        Guid? sizeId,
        Guid? colorId,
        Guid? setTypeId)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.AgeGroup)
            .Include(x => x.Department)
            .Include(x => x.SetType)
            .Include(x => x.Images)
            .Include(x => x.Variants)
                .ThenInclude(x => x.Size)
            .Include(x => x.Variants)
                .ThenInclude(x => x.Color)
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(x =>
                x.ProductName.Contains(value) ||
                x.SKU.Contains(value));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(x =>
                x.DepartmentId == departmentId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == categoryId.Value);
        }

        if (gender.HasValue)
        {
            query = query.Where(x =>
                x.Gender == gender.Value);
        }

        if (ageGroupId.HasValue)
        {
            query = query.Where(x =>
                x.AgeGroupId == ageGroupId.Value);
        }

        if (sizeId.HasValue)
        {
            query = query.Where(x =>
                x.Variants.Any(v =>
                    v.SizeId == sizeId.Value &&
                    v.IsActive));
        }

        if (colorId.HasValue)
        {
            query = query.Where(x =>
                x.Variants.Any(v =>
                    v.ColorId == colorId.Value &&
                    v.IsActive));
        }

        if (setTypeId.HasValue)
        {
            query = query.Where(x =>
                x.SetTypeId == setTypeId.Value);
        }

        var products = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var result = products
            .Select(MapProduct)
            .ToList();

        return new CatalogProductsResultDto
        {
            Found = result.Count > 0,
            Message = result.Count > 0
                ? null
                : "NOT FOUND",
            Products = result
        };
    }

    public async Task<ProductResponseDto?> GetProductAsync(Guid id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.AgeGroup)
            .Include(x => x.Department)
            .Include(x => x.SetType)
            .Include(x => x.Images)
            .Include(x => x.Variants)
                .ThenInclude(x => x.Size)
            .Include(x => x.Variants)
                .ThenInclude(x => x.Color)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product is null)
            return null;

        return MapProduct(product);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _db.Categories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Brand>> GetBrandsAsync()
    {
        return await _db.Brands
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Size>> GetSizesAsync()
    {
        return await _db.Sizes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Color>> GetColorsAsync()
    {
        return await _db.Colors
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<AgeGroup>> GetAgeGroupsAsync()
    {
        return await _db.AgeGroups
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.MinAgeMonths)
            .ToListAsync();
    }

    public async Task<List<Department>> GetDepartmentsAsync()
    {
        return await _db.Departments
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<SetType>> GetSetTypesAsync()
    {
        return await _db.SetTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.PieceCount)
            .ThenBy(x => x.Name)
            .ToListAsync();
    }

    // =========================================================
    // PRODUCT SAVE / UPDATE
    // =========================================================

    public async Task<ProductResponseDto> SaveProductAsync(
    Guid? id,
    ProductRequestDto dto)
    {
        await ValidateProductAsync(id, dto);

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        try
        {
            Product product;

            if (id.HasValue)
            {
                product = await _db.Products
                    .Include(x => x.Variants)
                    .Include(x => x.Images)
                    .FirstOrDefaultAsync(x => x.Id == id.Value)
                    ?? throw new KeyNotFoundException("Product not found.");

                // Update main product
                UpdateProduct(product, dto);

                // Delete old variants
                if (product.Variants.Any())
                {
                    _db.ProductVariants.RemoveRange(product.Variants);
                }

                // Delete old images
                if (product.Images.Any())
                {
                    _db.ProductImages.RemoveRange(product.Images);
                }

                // Save parent update + old child deletes first
                await _db.SaveChangesAsync();

                // Add new variants
                AddProductVariants(product, dto);

                // Add new images
                AddProductImages(product, dto);
            }
            else
            {
                product = CreateProduct(dto);

                await _db.Products.AddAsync(product);

                AddProductVariants(product, dto);
                AddProductImages(product, dto);
            }

            // Recalculate stock
            product.StockQuantity =
                product.Variants.Sum(x => x.StockQuantity);

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return (await GetProductAsync(product.Id))
                ?? throw new InvalidOperationException(
                    "Product was saved but could not be loaded.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================================================
    // PRODUCT VALIDATION
    // =========================================================

    private async Task ValidateProductAsync(
        Guid? id,
        ProductRequestDto dto)
    {
        if (dto is null)
        {
            throw new InvalidOperationException(
                "Product data is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.ProductName))
        {
            throw new InvalidOperationException(
                "Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.SKU))
        {
            throw new InvalidOperationException(
                "Product SKU is required.");
        }

        if (dto.CategoryId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Category is required.");
        }

        if (!await _db.Categories.AnyAsync(x =>
                x.Id == dto.CategoryId &&
                x.IsActive))
        {
            throw new InvalidOperationException(
                "Category does not exist or is inactive.");
        }

        if (dto.BrandId.HasValue &&
            !await _db.Brands.AnyAsync(x =>
                x.Id == dto.BrandId.Value &&
                x.IsActive))
        {
            throw new InvalidOperationException(
                "Brand does not exist or is inactive.");
        }

        if (dto.AgeGroupId.HasValue &&
            !await _db.AgeGroups.AnyAsync(x =>
                x.Id == dto.AgeGroupId.Value &&
                x.IsActive))
        {
            throw new InvalidOperationException(
                "Age group does not exist or is inactive.");
        }

        if (dto.DepartmentId.HasValue &&
            !await _db.Departments.AnyAsync(x =>
                x.Id == dto.DepartmentId.Value &&
                x.IsActive))
        {
            throw new InvalidOperationException(
                "Department does not exist or is inactive.");
        }

        if (dto.SetTypeId.HasValue &&
            !await _db.SetTypes.AnyAsync(x =>
                x.Id == dto.SetTypeId.Value &&
                x.IsActive))
        {
            throw new InvalidOperationException(
                "Set type does not exist or is inactive.");
        }

        if (dto.PurchasePrice < 0)
        {
            throw new InvalidOperationException(
                "Purchase price cannot be negative.");
        }

        if (dto.SalePrice < 0)
        {
            throw new InvalidOperationException(
                "Sale price cannot be negative.");
        }

        if (dto.Discount < 0)
        {
            throw new InvalidOperationException(
                "Discount cannot be negative.");
        }

        if (dto.MinimumStockLevel < 0)
        {
            throw new InvalidOperationException(
                "Minimum stock level cannot be negative.");
        }

        var productSku = dto.SKU.Trim();

        var duplicateProductSku =
            await _db.Products.AnyAsync(x =>
                x.SKU == productSku &&
                (!id.HasValue ||
                 x.Id != id.Value));

        if (duplicateProductSku)
        {
            throw new InvalidOperationException(
                "Product SKU already exists.");
        }

        if (dto.Variants is null)
        {
            throw new InvalidOperationException(
                "Product variants are required.");
        }

        var variantSkus = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var variant in dto.Variants)
        {
            if (variant.SizeId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Variant size is required.");
            }

            if (variant.ColorId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Variant color is required.");
            }

            if (string.IsNullOrWhiteSpace(variant.SKU))
            {
                throw new InvalidOperationException(
                    "Variant SKU is required.");
            }

            if (variant.PurchasePrice < 0)
            {
                throw new InvalidOperationException(
                    "Variant purchase price cannot be negative.");
            }

            if (variant.SalePrice < 0)
            {
                throw new InvalidOperationException(
                    "Variant sale price cannot be negative.");
            }

            if (variant.StockQuantity < 0)
            {
                throw new InvalidOperationException(
                    "Variant stock quantity cannot be negative.");
            }

            if (variant.MinimumStockLevel < 0)
            {
                throw new InvalidOperationException(
                    "Variant minimum stock level cannot be negative.");
            }

            var variantSku =
                variant.SKU.Trim();

            if (!variantSkus.Add(variantSku))
            {
                throw new InvalidOperationException(
                    $"Duplicate variant SKU: {variantSku}");
            }

            if (!await _db.Sizes.AnyAsync(x =>
                    x.Id == variant.SizeId &&
                    x.IsActive))
            {
                throw new InvalidOperationException(
                    "Variant size does not exist or is inactive.");
            }

            if (!await _db.Colors.AnyAsync(x =>
                    x.Id == variant.ColorId &&
                    x.IsActive))
            {
                throw new InvalidOperationException(
                    "Variant color does not exist or is inactive.");
            }

            var duplicateVariantSku =
                await _db.ProductVariants.AnyAsync(x =>
                    x.SKU == variantSku &&
                    (!id.HasValue ||
                     x.ProductId != id.Value));

            if (duplicateVariantSku)
            {
                throw new InvalidOperationException(
                    $"Variant SKU already exists: {variantSku}");
            }
        }

        if (id.HasValue)
        {
            var productExists =
                await _db.Products.AnyAsync(x =>
                    x.Id == id.Value);

            if (!productExists)
            {
                throw new KeyNotFoundException(
                    "Product not found.");
            }
        }
    }

    // =========================================================
    // PRODUCT CREATE
    // =========================================================

    private static Product CreateProduct(
        ProductRequestDto dto)
    {
        return new Product
        {
            Id = Guid.NewGuid(),

            ProductName =
                dto.ProductName.Trim(),

            SKU =
                dto.SKU.Trim(),

            Description =
                dto.Description?.Trim(),

            CategoryId =
                dto.CategoryId,

            BrandId =
                dto.BrandId,

            Gender =
                dto.Gender,

            AgeGroupId =
                dto.AgeGroupId,

            DepartmentId =
                dto.DepartmentId,

            SetTypeId =
                dto.SetTypeId,

            SetIncludes =
                dto.SetIncludes?.Trim(),

            Fabric =
                dto.Fabric?.Trim(),

            Season =
                dto.Season?.Trim(),

            PurchasePrice =
                dto.PurchasePrice,

            SalePrice =
                dto.SalePrice,

            Discount =
                dto.Discount,

            MinimumStockLevel =
                dto.MinimumStockLevel,

            IsActive =
                dto.IsActive,

            CreatedAt =
                DateTime.UtcNow
        };
    }

    // =========================================================
    // PRODUCT UPDATE
    // =========================================================

    private static void UpdateProduct(
        Product product,
        ProductRequestDto dto)
    {
        product.ProductName =
            dto.ProductName.Trim();

        product.SKU =
            dto.SKU.Trim();

        product.Description =
            dto.Description?.Trim();

        product.CategoryId =
            dto.CategoryId;

        product.BrandId =
            dto.BrandId;

        product.Gender =
            dto.Gender;

        product.AgeGroupId =
            dto.AgeGroupId;

        product.DepartmentId =
            dto.DepartmentId;

        product.SetTypeId =
            dto.SetTypeId;

        product.SetIncludes =
            dto.SetIncludes?.Trim();

        product.Fabric =
            dto.Fabric?.Trim();

        product.Season =
            dto.Season?.Trim();

        product.PurchasePrice =
            dto.PurchasePrice;

        product.SalePrice =
            dto.SalePrice;

        product.Discount =
            dto.Discount;

        product.MinimumStockLevel =
            dto.MinimumStockLevel;

        product.IsActive =
            dto.IsActive;

        product.UpdatedAt =
            DateTime.UtcNow;
    }

    // =========================================================
    // PRODUCT VARIANTS
    // =========================================================

    private static void AddProductVariants(
        Product product,
        ProductRequestDto dto)
    {
        foreach (var variantDto in dto.Variants)
        {
            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),

                ProductId =
                    product.Id,

                SizeId =
                    variantDto.SizeId,

                ColorId =
                    variantDto.ColorId,

                SKU =
                    variantDto.SKU.Trim(),

                PurchasePrice =
                    variantDto.PurchasePrice,

                SalePrice =
                    variantDto.SalePrice,

                StockQuantity =
                    variantDto.StockQuantity,

                MinimumStockLevel =
                    variantDto.MinimumStockLevel,

                IsActive =
                    variantDto.IsActive
            };

            product.Variants.Add(variant);
        }
    }

    // =========================================================
    // PRODUCT IMAGES
    // =========================================================

    private static void AddProductImages(
       Product product,
       ProductRequestDto dto)
    {
        var imageUrls = (dto.ImageUrls ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct()
            .ToList();

        for (int i = 0; i < imageUrls.Count; i++)
        {
            product.Images.Add(new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ImageUrl = imageUrls[i],
                IsPrimary = i == 0,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(x =>
                x.Id == id);

        if (product is null)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // =========================================================
    // CART
    // =========================================================

    public async Task<CartResponseDto> GetCartAsync(
        Guid userId)
    {
        var cart =
            await GetOrCreateCartAsync(userId);

        return await MapCartAsync(cart.Id);
    }

    public async Task<CartResponseDto> AddToCartAsync(
        Guid userId,
        AddToCartRequestDto dto)
    {
        if (dto.ProductVariantId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Product variant is required.");
        }

        if (dto.Quantity <= 0)
        {
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");
        }

        var variant = await _db.ProductVariants
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.Id == dto.ProductVariantId &&
                x.IsActive);

        if (variant?.Product is null ||
            !variant.Product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product variant not found or inactive.");
        }

        if (variant.StockQuantity < dto.Quantity)
        {
            throw new InvalidOperationException(
                "Insufficient stock.");
        }

        var cart =
            await GetOrCreateCartAsync(userId);

        var item = await _db.CartItems
            .FirstOrDefaultAsync(x =>
                x.CartId == cart.Id &&
                x.ProductVariantId ==
                dto.ProductVariantId);

        if (item is null)
        {
            item = new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                UserId = userId,
                ProductVariantId =
                    dto.ProductVariantId,
                Quantity = dto.Quantity,
                UnitPrice =
                    variant.SalePrice,
                ProductId =
                    variant.ProductId.ToString(),
                Name =
                    variant.Product.ProductName,
                AddedAt =
                    DateTime.UtcNow
            };

            await _db.CartItems.AddAsync(item);
        }
        else
        {
            var newQuantity =
                item.Quantity + dto.Quantity;

            if (variant.StockQuantity <
                newQuantity)
            {
                throw new InvalidOperationException(
                    "Insufficient stock.");
            }

            item.Quantity =
                newQuantity;

            item.UnitPrice =
                variant.SalePrice;
        }

        cart.UpdatedAt =
            DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await MapCartAsync(cart.Id);
    }

    public async Task<CartResponseDto> UpdateCartAsync(
        Guid userId,
        Guid itemId,
        int quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");
        }

        var item = await _db.CartItems
            .Include(x => x.ProductVariant)
            .FirstOrDefaultAsync(x =>
                x.Id == itemId &&
                x.UserId == userId);

        if (item is null)
        {
            throw new KeyNotFoundException(
                "Cart item not found.");
        }

        if (item.ProductVariant is null ||
            !item.ProductVariant.IsActive)
        {
            throw new KeyNotFoundException(
                "Product variant not found or inactive.");
        }

        if (item.ProductVariant.StockQuantity <
            quantity)
        {
            throw new InvalidOperationException(
                "Insufficient stock.");
        }

        item.Quantity =
            quantity;

        item.UnitPrice =
            item.ProductVariant.SalePrice;

        await _db.SaveChangesAsync();

        return await MapCartAsync(item.CartId);
    }

    public async Task<CartResponseDto> RemoveCartItemAsync(
        Guid userId,
        Guid itemId)
    {
        var item = await _db.CartItems
            .FirstOrDefaultAsync(x =>
                x.Id == itemId &&
                x.UserId == userId);

        if (item is null)
        {
            throw new KeyNotFoundException(
                "Cart item not found.");
        }

        var cartId =
            item.CartId;

        _db.CartItems.Remove(item);

        await _db.SaveChangesAsync();

        return await MapCartAsync(cartId);
    }

    public async Task ClearCartAsync(
        Guid userId)
    {
        var items = await _db.CartItems
            .Where(x =>
                x.UserId == userId)
            .ToListAsync();

        if (items.Count == 0)
            return;

        _db.CartItems.RemoveRange(items);

        await _db.SaveChangesAsync();
    }

    // =========================================================
    // ORDERS
    // =========================================================

    public async Task<OrderResponseDto> CreateOrderAsync(
        Guid userId,
        CreateOrderRequestDto dto)
    {
        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var address =
                await _db.Addresses.FirstOrDefaultAsync(x =>
                    x.Id == dto.ShippingAddressId &&
                    x.UserId == userId);

            if (address is null)
            {
                throw new KeyNotFoundException(
                    "Shipping address not found.");
            }

            var cart =
                await _db.Carts.FirstOrDefaultAsync(x =>
                    x.UserId == userId);

            if (cart is null)
            {
                throw new InvalidOperationException(
                    "Cart is empty.");
            }

            var items =
                await _db.CartItems
                    .Include(x => x.ProductVariant)
                        .ThenInclude(x => x!.Product)
                    .Where(x =>
                        x.CartId == cart.Id)
                    .ToListAsync();

            if (items.Count == 0)
            {
                throw new InvalidOperationException(
                    "Your cart is empty.");
            }

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

                if (item.ProductVariant.StockQuantity <
                    item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for {item.ProductVariant.Product.ProductName}.");
                }
            }

            var subtotal =
                items.Sum(x =>
                    x.ProductVariant!.SalePrice *
                    x.Quantity);

            var shippingAmount =
                dto.ShippingAmount < 0
                    ? 0
                    : dto.ShippingAmount;

            var order = new Order
            {
                Id = Guid.NewGuid(),

                UserId =
                    userId,

                OrderNumber =
                    await NextNumberAsync("ORD"),

                TotalAmount =
                    subtotal,

                DiscountAmount =
                    0,

                ShippingAmount =
                    shippingAmount,

                FinalAmount =
                    subtotal +
                    shippingAmount,

                PaymentMethod =
                    dto.PaymentMethod,

                PaymentStatus =
                    PaymentStatus.Pending,

                OrderStatus =
                    OrderStatus.Pending,

                ShippingAddressId =
                    address.Id,

                CreatedAt =
                    DateTime.UtcNow
            };

            foreach (var cartItem in items)
            {
                var variant =
                    cartItem.ProductVariant!;

                variant.StockQuantity -=
                    cartItem.Quantity;

                _db.StockTransactions.Add(
                    new StockTransaction
                    {
                        Id = Guid.NewGuid(),

                        ProductVariantId =
                            variant.Id,

                        TransactionType =
                            StockTransactionType.Sale,

                        Quantity =
                            cartItem.Quantity,

                        ReferenceType =
                            "Order",

                        ReferenceId =
                            order.Id,

                        Notes =
                            $"Order {order.OrderNumber}",

                        CreatedBy =
                            userId,

                        CreatedAt =
                            DateTime.UtcNow
                    });

                order.Items.Add(
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),

                        OrderId =
                            order.Id,

                        ProductVariantId =
                            variant.Id,

                        ProductName =
                            variant.Product!.ProductName,

                        SKU =
                            variant.SKU,

                        Quantity =
                            cartItem.Quantity,

                        UnitPrice =
                            variant.SalePrice,

                        TotalPrice =
                            variant.SalePrice *
                            cartItem.Quantity
                    });
            }

            order.Invoice =
                new Invoice
                {
                    Id = Guid.NewGuid(),

                    OrderId =
                        order.Id,

                    InvoiceNumber =
                        await NextNumberAsync("INV"),

                    InvoiceDate =
                        DateTime.UtcNow,

                    Status =
                        "Issued",

                    CreatedAt =
                        DateTime.UtcNow
                };

            if (dto.PaymentMethod ==
                PaymentMethod.OnlineBankTransfer)
            {
                if (!dto.PaymentProofId.HasValue)
                {
                    throw new InvalidOperationException(
                        "Payment proof is required for online bank transfer.");
                }

                var proof =
                    await _db.PaymentProofs
                        .FirstOrDefaultAsync(x =>
                            x.Id ==
                            dto.PaymentProofId.Value &&
                            x.UserId ==
                            userId);

                if (proof is null)
                {
                    throw new InvalidOperationException(
                        "Payment proof was not found or does not belong to the current user.");
                }
            }

            var payment =
                new Payment
                {
                    Id = Guid.NewGuid(),

                    OrderId =
                        order.Id,

                    PaymentMethod =
                        dto.PaymentMethod,

                    Amount =
                        order.FinalAmount,

                    PaymentStatus =
                        PaymentStatus.Pending,

                    BankName =
                        dto.BankName,

                    TransactionReference =
                        dto.TransactionReference,

                    PaymentProofUrl =
                        dto.PaymentProofId.HasValue
                            ? $"/api/payment-proof/{dto.PaymentProofId.Value}"
                            : dto.PaymentProofUrl,

                    Provider =
                        dto.PaymentMethod ==
                        PaymentMethod.CashOnDelivery
                            ? "Cash"
                            : "Manual",

                    CreatedAt =
                        DateTime.UtcNow
                };

            if (dto.PaymentMethod ==
                PaymentMethod.Card)
            {
                var gatewayResult =
                    await _gateway.ChargeAsync(
                        order.FinalAmount,
                        order.OrderNumber);

                payment.PaymentStatus =
                    gatewayResult.Success
                        ? PaymentStatus.Paid
                        : PaymentStatus.Failed;

                payment.Provider =
                    "MockGateway";

                payment.TransactionReference =
                    gatewayResult.TransactionReference;

                order.PaymentStatus =
                    payment.PaymentStatus;

                if (payment.PaymentStatus ==
                    PaymentStatus.Paid)
                {
                    payment.PaidAt =
                        DateTime.UtcNow;
                }
            }

            _db.Orders.Add(order);
            _db.Payments.Add(payment);
            _db.CartItems.RemoveRange(items);

            cart.UpdatedAt =
                DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            return (await GetOrderAsync(
                userId,
                order.Id,
                false))!;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrderResponseDto>> GetMyOrdersAsync(
        Guid userId)
    {
        var orders =
            await _db.Orders
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Invoice)
                .Include(x => x.Items)
                .Where(x =>
                    x.UserId == userId)
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ToListAsync();

        return orders
            .Select(MapOrder)
            .ToList();
    }

    public async Task<OrderResponseDto?> GetOrderAsync(
        Guid userId,
        Guid id,
        bool admin)
    {
        var query =
            _db.Orders
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Invoice)
                .Include(x => x.Items)
                .AsQueryable();

        if (!admin)
        {
            query =
                query.Where(x =>
                    x.UserId == userId);
        }

        var order =
            await query.FirstOrDefaultAsync(x =>
                x.Id == id);

        if (order is null)
            return null;

        return MapOrder(order);
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders =
            await _db.Orders
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Invoice)
                .Include(x => x.Items)
                .OrderByDescending(x =>
                    x.CreatedAt)
                .ToListAsync();

        return orders
            .Select(MapOrder)
            .ToList();
    }

    public async Task UpdateOrderStatusAsync(
        Guid id,
        OrderStatus status)
    {
        var order =
            await _db.Orders.FirstOrDefaultAsync(x =>
                x.Id == id);

        if (order is null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        if (order.OrderStatus == status)
            return;

        if (status == OrderStatus.Cancelled &&
            order.OrderStatus != OrderStatus.Cancelled)
        {
            var items =
                await _db.OrderItems
                    .Where(x =>
                        x.OrderId == id)
                    .ToListAsync();

            foreach (var item in items)
            {
                var variant =
                    await _db.ProductVariants
                        .FirstOrDefaultAsync(x =>
                            x.Id ==
                            item.ProductVariantId);

                if (variant is null)
                    continue;

                variant.StockQuantity +=
                    item.Quantity;

                _db.StockTransactions.Add(
                    new StockTransaction
                    {
                        Id = Guid.NewGuid(),

                        ProductVariantId =
                            variant.Id,

                        TransactionType =
                            StockTransactionType.Return,

                        Quantity =
                            item.Quantity,

                        ReferenceType =
                            "OrderCancel",

                        ReferenceId =
                            id,

                        Notes =
                            "Stock restored after order cancellation.",

                        CreatedAt =
                            DateTime.UtcNow
                    });
            }
        }

        order.OrderStatus =
            status;

        order.UpdatedAt =
            DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    // =========================================================
    // PAYMENTS
    // =========================================================

    public async Task<List<PaymentResponseDto>> GetPaymentsAsync()
    {
        return await _db.Payments
            .AsNoTracking()
            .Include(x => x.Order)
            .OrderByDescending(x =>
                x.CreatedAt)
            .Select(x =>
                new PaymentResponseDto
                {
                    Id =
                        x.Id,

                    OrderId =
                        x.OrderId,

                    OrderNumber =
                        x.Order != null
                            ? x.Order.OrderNumber
                            : "",

                    PaymentMethod =
                        x.PaymentMethod,

                    Amount =
                        x.Amount,

                    PaymentStatus =
                        x.PaymentStatus,

                    TransactionReference =
                        x.TransactionReference,

                    Provider =
                        x.Provider,

                    BankName =
                        x.BankName,

                    PaymentProofUrl =
                        x.PaymentProofUrl,

                    PaidAt =
                        x.PaidAt
                })
            .ToListAsync();
    }

    public async Task UpdatePaymentStatusAsync(
        Guid id,
        PaymentStatus status)
    {
        var payment =
            await _db.Payments
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (payment is null)
        {
            throw new KeyNotFoundException(
                "Payment not found.");
        }

        payment.PaymentStatus =
            status;

        if (status == PaymentStatus.Paid)
        {
            payment.PaidAt =
                DateTime.UtcNow;
        }
        else
        {
            payment.PaidAt = null;
        }

        if (payment.Order is not null)
        {
            payment.Order.PaymentStatus =
                status;

            payment.Order.UpdatedAt =
                DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    // =========================================================
    // ADDRESSES
    // =========================================================

    public async Task<List<Address>> GetAddressesAsync(
        Guid userId)
    {
        return await _db.Addresses
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId)
            .OrderByDescending(x =>
                x.IsDefault)
            .ThenByDescending(x =>
                x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Address> AddAddressAsync(
        Guid userId,
        AddressRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AddressLine))
        {
            throw new InvalidOperationException(
                "Address is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.City))
        {
            throw new InvalidOperationException(
                "City is required.");
        }

        if (dto.IsDefault)
        {
            await _db.Addresses
                .Where(x =>
                    x.UserId == userId)
                .ExecuteUpdateAsync(x =>
                    x.SetProperty(
                        p => p.IsDefault,
                        false));
        }

        var hasAddress =
            await _db.Addresses.AnyAsync(
                x => x.UserId == userId);

        var address =
            new Address
            {
                Id = Guid.NewGuid(),

                UserId =
                    userId,

                AddressLine =
                    dto.AddressLine.Trim(),

                City =
                    dto.City.Trim(),

                Area =
                    dto.Area?.Trim(),

                PostalCode =
                    dto.PostalCode?.Trim(),

                Country =
                    string.IsNullOrWhiteSpace(
                        dto.Country)
                        ? "Pakistan"
                        : dto.Country.Trim(),

                IsDefault =
                    dto.IsDefault ||
                    !hasAddress,

                CreatedAt =
                    DateTime.UtcNow
            };

        _db.Addresses.Add(address);

        await _db.SaveChangesAsync();

        return address;
    }

    public async Task DeleteAddressAsync(
        Guid userId,
        Guid id)
    {
        var address =
            await _db.Addresses
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId);

        if (address is null)
        {
            throw new KeyNotFoundException(
                "Address not found.");
        }

        _db.Addresses.Remove(address);

        await _db.SaveChangesAsync();
    }

    // =========================================================
    // DASHBOARD
    // =========================================================

    public async Task<DashboardResponseDto> GetDashboardAsync()
    {
        var today =
            DateTime.UtcNow.Date;

        var month =
            new DateTime(
                today.Year,
                today.Month,
                1);

        return new DashboardResponseDto
        {
            TotalUsers =
                await _db.Users.CountAsync(x =>
                    x.Role == "User"),

            TotalProducts =
                await _db.Products.CountAsync(x =>
                    x.IsActive),

            TotalOrders =
                await _db.Orders.CountAsync(),

            PendingOrders =
                await _db.Orders.CountAsync(x =>
                    x.OrderStatus ==
                    OrderStatus.Pending),

            TodaysSales =
                await _db.Orders
                    .Where(x =>
                        x.CreatedAt >= today &&
                        x.PaymentStatus ==
                        PaymentStatus.Paid)
                    .SumAsync(x =>
                        (decimal?)x.FinalAmount) ?? 0,

            MonthlySales =
                await _db.Orders
                    .Where(x =>
                        x.CreatedAt >= month &&
                        x.PaymentStatus ==
                        PaymentStatus.Paid)
                    .SumAsync(x =>
                        (decimal?)x.FinalAmount) ?? 0,

            TotalRevenue =
                await _db.Orders
                    .Where(x =>
                        x.PaymentStatus ==
                        PaymentStatus.Paid)
                    .SumAsync(x =>
                        (decimal?)x.FinalAmount) ?? 0,

            TotalPurchases =
                await _db.Purchases
                    .SumAsync(x =>
                        (decimal?)x.TotalAmount) ?? 0,

            LowStockProducts =
                await _db.ProductVariants
                    .CountAsync(x =>
                        x.IsActive &&
                        x.StockQuantity <=
                        x.MinimumStockLevel),

            PendingPayments =
                await _db.Payments
                    .CountAsync(x =>
                        x.PaymentStatus ==
                        PaymentStatus.Pending)
        };
    }

    // =========================================================
    // USERS
    // =========================================================

    public async Task<List<UserResponseDto>> GetUsersAsync()
    {
        return await _db.Users
            .AsNoTracking()
            .Select(x =>
                new UserResponseDto
                {
                    Id =
                        x.Id,

                    Username =
                        x.UserName ?? "",

                    Email =
                        x.Email,

                    PhoneNumber =
                        x.PhoneNumber,

                    FullName =
                        x.FullName,

                    Role =
                        x.Role
                })
            .ToListAsync();
    }

    public async Task SetUserActiveAsync(
        Guid id,
        bool active)
    {
        var user =
            await _db.Users.FirstOrDefaultAsync(x =>
                x.Id == id);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User not found.");
        }

        user.IsActive =
            active;

        await _db.SaveChangesAsync();
    }

    // =========================================================
    // PURCHASES
    // =========================================================

    public async Task<Purchase> CreatePurchaseAsync(
        PurchaseRequestDto dto)
    {
        if (dto.SupplierId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Supplier is required.");
        }

        if (dto.Items is null ||
            dto.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "Purchase must contain at least one item.");
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var supplier =
                await _db.Suppliers
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.SupplierId &&
                        x.IsActive);

            if (supplier is null)
            {
                throw new KeyNotFoundException(
                    "Supplier not found.");
            }

            var purchase =
                new Purchase
                {
                    Id = Guid.NewGuid(),

                    SupplierId =
                        dto.SupplierId,

                    PurchaseNumber =
                        await NextNumberAsync("PUR"),

                    InvoiceNumber =
                        dto.InvoiceNumber?.Trim(),

                    PurchaseDate =
                        dto.PurchaseDate,

                    PaymentStatus =
                        dto.PaymentStatus,

                    Notes =
                        dto.Notes?.Trim(),

                    CreatedAt =
                        DateTime.UtcNow
                };

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.ProductVariantId ==
                    Guid.Empty)
                {
                    throw new InvalidOperationException(
                        "Product variant is required.");
                }

                if (itemDto.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        "Purchase quantity must be greater than zero.");
                }

                if (itemDto.PurchasePrice < 0)
                {
                    throw new InvalidOperationException(
                        "Purchase price cannot be negative.");
                }

                var variant =
                    await _db.ProductVariants
                        .FirstOrDefaultAsync(x =>
                            x.Id ==
                            itemDto.ProductVariantId);

                if (variant is null)
                {
                    throw new KeyNotFoundException(
                        "Product variant not found.");
                }

                variant.StockQuantity +=
                    itemDto.Quantity;

                var total =
                    itemDto.PurchasePrice *
                    itemDto.Quantity;

                purchase.Items.Add(
                    new PurchaseItem
                    {
                        Id = Guid.NewGuid(),

                        PurchaseId =
                            purchase.Id,

                        ProductVariantId =
                            variant.Id,

                        Quantity =
                            itemDto.Quantity,

                        PurchasePrice =
                            itemDto.PurchasePrice,

                        TotalPrice =
                            total
                    });

                _db.StockTransactions.Add(
                    new StockTransaction
                    {
                        Id = Guid.NewGuid(),

                        ProductVariantId =
                            variant.Id,

                        TransactionType =
                            StockTransactionType.Purchase,

                        Quantity =
                            itemDto.Quantity,

                        ReferenceType =
                            "Purchase",

                        ReferenceId =
                            purchase.Id,

                        Notes =
                            $"Purchase {purchase.PurchaseNumber}",

                        CreatedAt =
                            DateTime.UtcNow
                    });
            }

            purchase.TotalAmount =
                purchase.Items.Sum(x =>
                    x.TotalPrice);

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

    // =========================================================
    // SUPPLIERS
    // =========================================================

    public async Task<List<Supplier>> GetSuppliersAsync()
    {
        return await _db.Suppliers
            .AsNoTracking()
            .OrderBy(x =>
                x.Name)
            .ToListAsync();
    }

    public async Task<Supplier> SaveSupplierAsync(
        Guid? id,
        SupplierRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException(
                "Supplier name is required.");
        }

        Supplier supplier;

        if (id.HasValue)
        {
            supplier =
                await _db.Suppliers
                    .FirstOrDefaultAsync(x =>
                        x.Id == id.Value)
                ?? throw new KeyNotFoundException(
                    "Supplier not found.");
        }
        else
        {
            supplier =
                new Supplier
                {
                    Id = Guid.NewGuid(),
                    CreatedAt =
                        DateTime.UtcNow
                };

            await _db.Suppliers.AddAsync(
                supplier);
        }

        supplier.Name =
            dto.Name.Trim();

        supplier.Phone =
            dto.Phone?.Trim();

        supplier.Email =
            dto.Email?.Trim();

        supplier.Address =
            dto.Address?.Trim();

        supplier.IsActive =
            dto.IsActive;

        await _db.SaveChangesAsync();

        return supplier;
    }

    // =========================================================
    // STOCK
    // =========================================================

    public async Task<List<object>> GetStockHistoryAsync(
        Guid variantId)
    {
        return await _db.StockTransactions
            .AsNoTracking()
            .Where(x =>
                x.ProductVariantId ==
                variantId)
            .OrderByDescending(x =>
                x.CreatedAt)
            .Select(x =>
                (object)new
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

    // =========================================================
    // PRIVATE CART METHODS
    // =========================================================

    private async Task<Cart> GetOrCreateCartAsync(
        Guid userId)
    {
        var cart =
            await _db.Carts
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId);

        if (cart is not null)
            return cart;

        cart =
            new Cart
            {
                Id = Guid.NewGuid(),

                UserId =
                    userId,

                CreatedAt =
                    DateTime.UtcNow,

                UpdatedAt =
                    DateTime.UtcNow
            };

        await _db.Carts.AddAsync(cart);

        await _db.SaveChangesAsync();

        return cart;
    }

    private async Task<CartResponseDto> MapCartAsync(
        Guid cartId)
    {
        var cart =
            await _db.Carts
                .AsNoTracking()
                .Include(x => x.Items)
                    .ThenInclude(x =>
                        x.ProductVariant)
                            .ThenInclude(x =>
                                x!.Product)
                                    .ThenInclude(x =>
                                        x!.Images)
                .Include(x => x.Items)
                    .ThenInclude(x =>
                        x.ProductVariant)
                            .ThenInclude(x =>
                                x!.Size)
                .Include(x => x.Items)
                    .ThenInclude(x =>
                        x.ProductVariant)
                            .ThenInclude(x =>
                                x!.Color)
                .FirstOrDefaultAsync(x =>
                    x.Id == cartId);

        if (cart is null)
        {
            return new CartResponseDto
            {
                CartId =
                    cartId,

                Items =
                    new List<CartItemResponseDto>(),

                TotalQuantity =
                    0,

                TotalAmount =
                    0
            };
        }

        var items =
            cart.Items
                .Where(x =>
                    x.ProductVariant != null &&
                    x.ProductVariant.Product != null)
                .Select(x =>
                    new CartItemResponseDto
                    {
                        Id =
                            x.Id,

                        ProductVariantId =
                            x.ProductVariantId,

                        ProductName =
                            x.ProductVariant!
                                .Product!
                                .ProductName,

                        SKU =
                            x.ProductVariant
                                .SKU,

                        ImageUrl =
                            x.ProductVariant
                                .Product!
                                .Images
                                .FirstOrDefault(i =>
                                    i.IsPrimary)
                                ?.ImageUrl
                            ??
                            x.ProductVariant
                                .Product!
                                .Images
                                .FirstOrDefault()
                                ?.ImageUrl,

                        Size =
                            x.ProductVariant
                                .Size?.Name ?? "",

                        Color =
                            x.ProductVariant
                                .Color?.Name ?? "",

                        UnitPrice =
                            x.ProductVariant
                                .SalePrice,

                        Quantity =
                            x.Quantity,

                        Total =
                            x.ProductVariant
                                .SalePrice *
                            x.Quantity
                    })
                .ToList();

        return new CartResponseDto
        {
            CartId =
                cart.Id,

            Items =
                items,

            TotalQuantity =
                items.Sum(x =>
                    x.Quantity),

            TotalAmount =
                items.Sum(x =>
                    x.Total)
        };
    }

    // =========================================================
    // PRIVATE PRODUCT MAPPER
    // =========================================================

    private static ProductResponseDto MapProduct(
        Product product)
    {
        return new ProductResponseDto
        {
            Id =
                product.Id,

            ProductName =
                product.ProductName,

            SKU =
                product.SKU,

            Description =
                product.Description,

            CategoryId =
                product.CategoryId,

            CategoryName =
                product.Category?.Name,

            BrandId =
                product.BrandId,

            BrandName =
                product.Brand?.Name,

            Gender =
                product.Gender,

            AgeGroupId =
                product.AgeGroupId,

            AgeGroupName =
                product.AgeGroup?.Name,

            DepartmentId =
                product.DepartmentId,

            DepartmentName =
                product.Department?.Name,

            SetTypeId =
                product.SetTypeId,

            SetTypeName =
                product.SetType?.Name,

            SetIncludes =
                product.SetIncludes,

            Fabric =
                product.Fabric,

            Season =
                product.Season,

            PurchasePrice =
                product.PurchasePrice,

            SalePrice =
                product.SalePrice,

            Discount =
                product.Discount,

            StockQuantity =
                product.Variants
                    .Where(x =>
                        x.IsActive)
                    .Sum(x =>
                        x.StockQuantity),

            MinimumStockLevel =
                product.MinimumStockLevel,

            IsActive =
                product.IsActive,

            Images =
                product.Images
                    .OrderByDescending(x =>
                        x.IsPrimary)
                    .ThenBy(x =>
                        x.CreatedAt)
                    .Select(x =>
                        x.ImageUrl)
                    .ToList(),

            Variants =
                product.Variants
                    .Select(v =>
                        new ProductVariantResponseDto
                        {
                            Id =
                                v.Id,

                            SizeId =
                                v.SizeId,

                            SizeName =
                                v.Size?.Name ??
                                "",

                            ColorId =
                                v.ColorId,

                            ColorName =
                                v.Color?.Name ??
                                "",

                            SKU =
                                v.SKU,

                            PurchasePrice =
                                v.PurchasePrice,

                            SalePrice =
                                v.SalePrice,

                            StockQuantity =
                                v.StockQuantity,

                            MinimumStockLevel =
                                v.MinimumStockLevel,

                            IsActive =
                                v.IsActive
                        })
                    .ToList()
        };
    }

    // =========================================================
    // PRIVATE ORDER MAPPER
    // =========================================================

    private static OrderResponseDto MapOrder(
        Order order)
    {
        return new OrderResponseDto
        {
            Id =
                order.Id,

            OrderNumber =
                order.OrderNumber,

            InvoiceNumber =
                order.Invoice?.InvoiceNumber,

            UserId =
                order.UserId,

            CustomerName =
                order.User?.FullName,

            TotalAmount =
                order.TotalAmount,

            DiscountAmount =
                order.DiscountAmount,

            ShippingAmount =
                order.ShippingAmount,

            FinalAmount =
                order.FinalAmount,

            PaymentMethod =
                order.PaymentMethod,

            PaymentStatus =
                order.PaymentStatus,

            OrderStatus =
                order.OrderStatus,

            CreatedAt =
                order.CreatedAt,

            Items =
                order.Items
                    .Select(i =>
                        new OrderItemResponseDto
                        {
                            ProductVariantId =
                                i.ProductVariantId,

                            ProductName =
                                i.ProductName,

                            SKU =
                                i.SKU,

                            Quantity =
                                i.Quantity,

                            UnitPrice =
                                i.UnitPrice,

                            TotalPrice =
                                i.TotalPrice
                        })
                    .ToList()
        };
    }

    // =========================================================
    // NUMBER GENERATOR
    // =========================================================

    private async Task<string> NextNumberAsync(
        string prefix)
    {
        var now =
            DateTime.UtcNow;

        var date =
            now.ToString("yyyyMMdd");

        int count;

        switch (prefix)
        {
            case "ORD":
                count =
                    await _db.Orders
                        .CountAsync(x =>
                            x.CreatedAt.Date ==
                            now.Date) + 1;
                break;

            case "INV":
                count =
                    await _db.Invoices
                        .CountAsync(x =>
                            x.CreatedAt.Date ==
                            now.Date) + 1;
                break;

            case "PUR":
                count =
                    await _db.Purchases
                        .CountAsync(x =>
                            x.CreatedAt.Date ==
                            now.Date) + 1;
                break;

            default:
                count = 1;
                break;
        }

        return
            $"{prefix}-{date}-{count:0000}";
    }
}
