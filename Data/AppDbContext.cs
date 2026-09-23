using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();
    public DbSet<MasterRecord> MasterRecords => Set<MasterRecord>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormDefinition>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedNever(); // formId is assigned explicitly (1047, 2001, ...)
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
            e.HasOne(r => r.Form)
                .WithMany(f => f.Records)
                .HasForeignKey(r => r.FormId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => r.FormId);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(100).IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.FullName).HasMaxLength(200);
            e.Property(u => u.Role).HasMaxLength(50);
        });

        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Price).HasColumnType("decimal(18,2)");
            e.HasOne(c => c.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.OrderNo).HasMaxLength(50);
            e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            e.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Price).HasColumnType("decimal(18,2)");
            e.HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}