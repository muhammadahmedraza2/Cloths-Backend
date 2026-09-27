using ClothingErp.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();
    public DbSet<MasterRecord> MasterRecords => Set<MasterRecord>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Size> Sizes => Set<Size>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<SetType> SetTypes => Set<SetType>();
    public DbSet<AgeGroup> AgeGroups => Set<AgeGroup>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PaymentProof> PaymentProofs => Set<PaymentProof>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("Users");
            e.Property(u => u.UserName).HasColumnName("Username").HasMaxLength(100).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            e.Property(u => u.Role).HasMaxLength(50).IsRequired();
            e.Property(u => u.PcId).HasColumnName("PcId").IsRequired();
            e.Property(u => u.IsActive).HasColumnName("IsActive").IsRequired();
            e.Property(u => u.CreatedAt).HasColumnName("CreatedAt").IsRequired();
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.PhoneNumber).HasMaxLength(50);
            e.HasIndex(u => u.UserName).IsUnique();
            e.HasIndex(u => u.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
        });

        modelBuilder.Entity<FormDefinition>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedNever();
            e.Property(f => f.Title).HasMaxLength(200).IsRequired();
            e.Property(f => f.Breadcrumb).HasMaxLength(400);
            e.Property(f => f.ColumnsJson).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<MasterRecord>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.DataJson).HasColumnType("nvarchar(max)");
            e.Property(r => r.Status).HasMaxLength(20);
            e.Property(r => r.Closed).HasMaxLength(1);
            e.Property(r => r.CreatedBy).HasMaxLength(100);
            e.HasOne(r => r.Form).WithMany(f => f.Records).HasForeignKey(r => r.FormId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => r.FormId);
        });

        ConfigureCatalog(modelBuilder);
        ConfigureCommerce(modelBuilder);
    }

    private static void ConfigureCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("ShopCategories");
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Brand>(e =>
        {
            e.ToTable("ShopBrands");
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Size>(e =>
        {
            e.ToTable("ShopSizes");
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Color>(e =>
        {
            e.ToTable("ShopColors");
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Department>(e =>
        {
            e.ToTable("ShopDepartments");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<SetType>(e =>
        {
            e.ToTable("ShopSetTypes");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<AgeGroup>(e =>
        {
            e.ToTable("ShopAgeGroups");
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("ShopProducts");
            e.Property(x => x.ProductName).HasMaxLength(250).IsRequired();
            e.Property(x => x.SKU).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.SKU).IsUnique();
            e.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            e.Property(x => x.SalePrice).HasPrecision(18, 2);
            e.Property(x => x.Discount).HasPrecision(18, 2);
            e.Property(x => x.SetIncludes).HasMaxLength(300);

            e.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Brand).WithMany(x => x.Products).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.AgeGroup).WithMany(x => x.Products).HasForeignKey(x => x.AgeGroupId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Department).WithMany(x => x.Products).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.SetType).WithMany(x => x.Products).HasForeignKey(x => x.SetTypeId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.ToTable("ShopProductVariants");
            e.Property(x => x.SKU).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.SKU).IsUnique();
            e.HasIndex(x => new { x.ProductId, x.SizeId, x.ColorId }).IsUnique();
            e.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            e.Property(x => x.SalePrice).HasPrecision(18, 2);
            e.HasOne(x => x.Product).WithMany(x => x.Variants).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Size).WithMany(x => x.Variants).HasForeignKey(x => x.SizeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Color).WithMany(x => x.Variants).HasForeignKey(x => x.ColorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductImage>(e =>
        {
            e.ToTable("ShopProductImages");
            e.Property(x => x.ImageUrl).HasMaxLength(1000).IsRequired();
            e.HasOne(x => x.Product).WithMany(x => x.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCommerce(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(e =>
        {
            e.ToTable("ShopCarts");
            e.HasIndex(x => x.UserId).IsUnique();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.ToTable("ShopCartItems");
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.HasIndex(x => new { x.CartId, x.ProductVariantId }).IsUnique();
            e.HasOne(x => x.Cart).WithMany(x => x.Items).HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ProductVariant).WithMany(x => x.CartItems).HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User).WithMany(x => x.CartItems).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Address>(e =>
        {
            e.ToTable("ShopAddresses");
            e.Property(x => x.AddressLine).HasMaxLength(500).IsRequired();
            e.Property(x => x.City).HasMaxLength(100).IsRequired();
            e.Property(x => x.Area).HasMaxLength(100);
            e.Property(x => x.PostalCode).HasMaxLength(20);
            e.Property(x => x.Country).HasMaxLength(100);
            e.HasOne(x => x.User).WithMany(x => x.Addresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("ShopOrders");
            e.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            e.Property(x => x.ShippingAmount).HasPrecision(18, 2);
            e.Property(x => x.FinalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.User).WithMany(x => x.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ShippingAddress).WithMany().HasForeignKey(x => x.ShippingAddressId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("ShopOrderItems");
            e.Property(x => x.ProductName).HasMaxLength(250).IsRequired();
            e.Property(x => x.SKU).HasMaxLength(100).IsRequired();
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.TotalPrice).HasPrecision(18, 2);
            e.HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ProductVariant).WithMany(x => x.OrderItems).HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.ToTable("ShopPayments");
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.TransactionReference).HasMaxLength(200);
            e.Property(x => x.Provider).HasMaxLength(100);
            e.Property(x => x.BankName).HasMaxLength(150);
            e.Property(x => x.PaymentProofUrl).HasMaxLength(1000);
            e.HasOne(x => x.Order).WithMany(x => x.Payments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StockTransaction>(e =>
        {
            e.ToTable("ShopStockTransactions");
            e.HasIndex(x => x.ProductVariantId);
            e.HasOne(x => x.ProductVariant).WithMany(x => x.StockTransactions).HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Supplier>(e =>
        {
            e.ToTable("ShopSuppliers");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Purchase>(e =>
        {
            e.ToTable("ShopPurchases");
            e.Property(x => x.PurchaseNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.PurchaseNumber).IsUnique();
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasOne(x => x.Supplier).WithMany(x => x.Purchases).HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseItem>(e =>
        {
            e.ToTable("ShopPurchaseItems");
            e.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            e.Property(x => x.TotalPrice).HasPrecision(18, 2);
            e.HasOne(x => x.Purchase).WithMany(x => x.Items).HasForeignKey(x => x.PurchaseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ProductVariant).WithMany(x => x.PurchaseItems).HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Invoice>(e =>
        {
            e.ToTable("ShopInvoices");
            e.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.InvoiceNumber).IsUnique();
            e.HasOne(x => x.Order).WithOne(x => x.Invoice).HasForeignKey<Invoice>(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentProof>(e =>
        {
            e.ToTable("ShopPaymentProofs");
            e.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("ShopRefreshTokens");
            e.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}