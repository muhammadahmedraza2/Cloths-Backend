using ClothingErp.Api.Models;

namespace ClothingErp.Api.Dtos;

public class CategoryRequestDto { public string Name { get; set; } = ""; public string? Description { get; set; } public string? ImageUrl { get; set; } public bool IsActive { get; set; } = true; }
public class BrandRequestDto { public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }
public class SizeRequestDto { public string Name { get; set; } = ""; public string? AgeRange { get; set; } public bool IsActive { get; set; } = true; }
public class ColorRequestDto { public string Name { get; set; } = ""; public string? HexCode { get; set; } public bool IsActive { get; set; } = true; }
public class AgeGroupRequestDto { public string Name { get; set; } = ""; public int? MinAgeMonths { get; set; } public int? MaxAgeMonths { get; set; } public bool IsActive { get; set; } = true; }
public class DepartmentRequestDto { public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }
public class SetTypeRequestDto { public string Name { get; set; } = ""; public int PieceCount { get; set; } = 1; public bool IsActive { get; set; } = true; }

public class ProductVariantRequestDto
{
    public Guid SizeId { get; set; }
    public Guid ColorId { get; set; }
    public string SKU { get; set; } = "";
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductRequestDto
{
    public string ProductName { get; set; } = "";
    public string SKU { get; set; } = "";
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? BrandId { get; set; }
    public Gender Gender { get; set; }
    public Guid? AgeGroupId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SetTypeId { get; set; }
    public string? SetIncludes { get; set; }
    public string? Fabric { get; set; }
    public string? Season { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Discount { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public List<string> ImageUrls { get; set; } = new();
    public List<ProductVariantRequestDto> Variants { get; set; } = new();
}

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = "";
    public string SKU { get; set; } = "";
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public Gender Gender { get; set; }
    public Guid? AgeGroupId { get; set; }
    public string? AgeGroupName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? SetTypeId { get; set; }
    public string? SetTypeName { get; set; }
    public string? SetIncludes { get; set; }
    public string? Fabric { get; set; }
    public string? Season { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal Discount { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsActive { get; set; }
    public List<string> Images { get; set; } = new();
    public List<ProductVariantResponseDto> Variants { get; set; } = new();
}
public class ProductVariantResponseDto
{
    public Guid Id { get; set; }
    public Guid SizeId { get; set; }
    public string SizeName { get; set; } = "";
    public Guid ColorId { get; set; }
    public string ColorName { get; set; } = "";
    public string SKU { get; set; } = "";
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinimumStockLevel { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Customer-facing product list wrapper — "Not Found" ke liye.</summary>
public class CatalogProductsResultDto
{
    public bool Found { get; set; }
    public string? Message { get; set; }
    public List<ProductResponseDto> Products { get; set; } = new();
}

public class AddToCartRequestDto { public Guid ProductVariantId { get; set; } public int Quantity { get; set; } = 1; }
public class UpdateCartRequestDto { public int Quantity { get; set; } }

public class CartItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = "";
    public string SKU { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string Size { get; set; } = "";
    public string Color { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
}
public class CartResponseDto
{
    public Guid CartId { get; set; }
    public List<CartItemResponseDto> Items { get; set; } = new();
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }
}

public class AddressRequestDto
{
    public string AddressLine { get; set; } = "";
    public string City { get; set; } = "";
    public string? Area { get; set; }
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Pakistan";
    public bool IsDefault { get; set; }
}

public class CreateOrderRequestDto
{
    public Guid ShippingAddressId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal ShippingAmount { get; set; }
    public string? BankName { get; set; }
    public string? TransactionReference { get; set; }
    public string? PaymentProofUrl { get; set; }
    public Guid? PaymentProofId { get; set; }
}

public class OrderItemResponseDto
{
    public Guid ProductVariantId { get; set; }
    public string ProductName { get; set; } = "";
    public string SKU { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string? InvoiceNumber { get; set; }
    public Guid UserId { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
}

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = "";
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? TransactionReference { get; set; }
    public string? Provider { get; set; }
    public string? BankName { get; set; }
    public string? PaymentProofUrl { get; set; }
    public Guid? PaymentProofId { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class UpdateOrderStatusDto { public OrderStatus Status { get; set; } }
public class UpdatePaymentStatusDto { public PaymentStatus Status { get; set; } }

public class SupplierRequestDto { public string Name { get; set; } = ""; public string? Phone { get; set; } public string? Email { get; set; } public string? Address { get; set; } public bool IsActive { get; set; } = true; }
public class PurchaseItemRequestDto { public Guid ProductVariantId { get; set; } public int Quantity { get; set; } public decimal PurchasePrice { get; set; } }
public class PurchaseRequestDto { public Guid SupplierId { get; set; } public string? InvoiceNumber { get; set; } public DateTime PurchaseDate { get; set; } = DateTime.UtcNow; public PurchasePaymentStatus PaymentStatus { get; set; } public string? Notes { get; set; } public List<PurchaseItemRequestDto> Items { get; set; } = new(); }

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

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
}