using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CLOTHS_ERP.API.Migrations;

public partial class AddKidsClothingCommerce : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
IF COL_LENGTH('Users','NormalizedUserName') IS NULL ALTER TABLE Users ADD NormalizedUserName nvarchar(256) NULL;
IF COL_LENGTH('Users','Email') IS NULL ALTER TABLE Users ADD Email nvarchar(256) NULL;
IF COL_LENGTH('Users','NormalizedEmail') IS NULL ALTER TABLE Users ADD NormalizedEmail nvarchar(256) NULL;
IF COL_LENGTH('Users','EmailConfirmed') IS NULL ALTER TABLE Users ADD EmailConfirmed bit NOT NULL CONSTRAINT DF_Users_EmailConfirmed DEFAULT 0;
IF COL_LENGTH('Users','PhoneNumber') IS NULL ALTER TABLE Users ADD PhoneNumber nvarchar(max) NULL;
IF COL_LENGTH('Users','PhoneNumberConfirmed') IS NULL ALTER TABLE Users ADD PhoneNumberConfirmed bit NOT NULL CONSTRAINT DF_Users_PhoneNumberConfirmed DEFAULT 0;
IF COL_LENGTH('Users','TwoFactorEnabled') IS NULL ALTER TABLE Users ADD TwoFactorEnabled bit NOT NULL CONSTRAINT DF_Users_TwoFactorEnabled DEFAULT 0;
IF COL_LENGTH('Users','LockoutEnd') IS NULL ALTER TABLE Users ADD LockoutEnd datetimeoffset NULL;
IF COL_LENGTH('Users','LockoutEnabled') IS NULL ALTER TABLE Users ADD LockoutEnabled bit NOT NULL CONSTRAINT DF_Users_LockoutEnabled DEFAULT 0;
IF COL_LENGTH('Users','AccessFailedCount') IS NULL ALTER TABLE Users ADD AccessFailedCount int NOT NULL CONSTRAINT DF_Users_AccessFailedCount DEFAULT 0;
IF COL_LENGTH('Users','SecurityStamp') IS NULL ALTER TABLE Users ADD SecurityStamp nvarchar(max) NULL;
IF COL_LENGTH('Users','ConcurrencyStamp') IS NULL ALTER TABLE Users ADD ConcurrencyStamp nvarchar(max) NULL;
IF COL_LENGTH('Users','IsActive') IS NULL ALTER TABLE Users ADD IsActive bit NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1;

IF OBJECT_ID('AspNetRoles') IS NULL
BEGIN
CREATE TABLE AspNetRoles (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_AspNetRoles PRIMARY KEY,
 Name nvarchar(256) NULL,
 NormalizedName nvarchar(256) NULL,
 ConcurrencyStamp nvarchar(max) NULL
);
CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;
END;

IF OBJECT_ID('AspNetUserClaims') IS NULL
BEGIN
CREATE TABLE AspNetUserClaims (
 Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AspNetUserClaims PRIMARY KEY,
 UserId uniqueidentifier NOT NULL,
 ClaimType nvarchar(max) NULL,
 ClaimValue nvarchar(max) NULL,
 CONSTRAINT FK_AspNetUserClaims_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims(UserId);
END;

IF OBJECT_ID('AspNetUserLogins') IS NULL
BEGIN
CREATE TABLE AspNetUserLogins (
 LoginProvider nvarchar(128) NOT NULL,
 ProviderKey nvarchar(128) NOT NULL,
 ProviderDisplayName nvarchar(max) NULL,
 UserId uniqueidentifier NOT NULL,
 CONSTRAINT PK_AspNetUserLogins PRIMARY KEY(LoginProvider, ProviderKey),
 CONSTRAINT FK_AspNetUserLogins_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins(UserId);
END;

IF OBJECT_ID('AspNetUserRoles') IS NULL
BEGIN
CREATE TABLE AspNetUserRoles (
 UserId uniqueidentifier NOT NULL,
 RoleId uniqueidentifier NOT NULL,
 CONSTRAINT PK_AspNetUserRoles PRIMARY KEY(UserId, RoleId),
 CONSTRAINT FK_AspNetUserRoles_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE,
 CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY(RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);
CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles(RoleId);
END;

IF OBJECT_ID('AspNetUserTokens') IS NULL
BEGIN
CREATE TABLE AspNetUserTokens (
 UserId uniqueidentifier NOT NULL,
 LoginProvider nvarchar(128) NOT NULL,
 Name nvarchar(128) NOT NULL,
 Value nvarchar(max) NULL,
 CONSTRAINT PK_AspNetUserTokens PRIMARY KEY(UserId, LoginProvider, Name),
 CONSTRAINT FK_AspNetUserTokens_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UserNameIndex' AND object_id=OBJECT_ID('Users'))
CREATE UNIQUE INDEX UserNameIndex ON Users(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='EmailIndex' AND object_id=OBJECT_ID('Users'))
CREATE INDEX EmailIndex ON Users(NormalizedEmail);

IF OBJECT_ID('ShopCategories') IS NULL
CREATE TABLE ShopCategories (Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopCategories PRIMARY KEY, Name nvarchar(150) NOT NULL, Description nvarchar(max) NULL, ImageUrl nvarchar(max) NULL, IsActive bit NOT NULL, CreatedAt datetime2 NOT NULL, UpdatedAt datetime2 NULL);
IF OBJECT_ID('ShopBrands') IS NULL
CREATE TABLE ShopBrands (Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopBrands PRIMARY KEY, Name nvarchar(150) NOT NULL, IsActive bit NOT NULL, CreatedAt datetime2 NOT NULL);
IF OBJECT_ID('ShopSizes') IS NULL
CREATE TABLE ShopSizes (Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopSizes PRIMARY KEY, Name nvarchar(80) NOT NULL, AgeRange nvarchar(max) NULL, IsActive bit NOT NULL);
IF OBJECT_ID('ShopColors') IS NULL
CREATE TABLE ShopColors (Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopColors PRIMARY KEY, Name nvarchar(80) NOT NULL, HexCode nvarchar(max) NULL, IsActive bit NOT NULL);
IF OBJECT_ID('ShopAgeGroups') IS NULL
CREATE TABLE ShopAgeGroups (Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopAgeGroups PRIMARY KEY, Name nvarchar(100) NOT NULL, MinAgeMonths int NULL, MaxAgeMonths int NULL, IsActive bit NOT NULL);

IF OBJECT_ID('ShopProducts') IS NULL
CREATE TABLE ShopProducts (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopProducts PRIMARY KEY, ProductName nvarchar(250) NOT NULL, SKU nvarchar(100) NOT NULL,
 Description nvarchar(max) NULL, CategoryId uniqueidentifier NOT NULL, BrandId uniqueidentifier NULL, Gender int NOT NULL, AgeGroupId uniqueidentifier NULL,
 Fabric nvarchar(max) NULL, Season nvarchar(max) NULL, PurchasePrice decimal(18,2) NOT NULL, SalePrice decimal(18,2) NOT NULL, Discount decimal(18,2) NOT NULL,
 StockQuantity int NOT NULL, MinimumStockLevel int NOT NULL, IsActive bit NOT NULL, CreatedAt datetime2 NOT NULL, UpdatedAt datetime2 NULL,
 CONSTRAINT FK_ShopProducts_ShopCategories_CategoryId FOREIGN KEY(CategoryId) REFERENCES ShopCategories(Id),
 CONSTRAINT FK_ShopProducts_ShopBrands_BrandId FOREIGN KEY(BrandId) REFERENCES ShopBrands(Id),
 CONSTRAINT FK_ShopProducts_ShopAgeGroups_AgeGroupId FOREIGN KEY(AgeGroupId) REFERENCES ShopAgeGroups(Id)
);
CREATE UNIQUE INDEX IX_ShopProducts_SKU ON ShopProducts(SKU);

IF OBJECT_ID('ShopProductVariants') IS NULL
CREATE TABLE ShopProductVariants (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopProductVariants PRIMARY KEY, ProductId uniqueidentifier NOT NULL, SizeId uniqueidentifier NOT NULL, ColorId uniqueidentifier NOT NULL,
 SKU nvarchar(100) NOT NULL, PurchasePrice decimal(18,2) NOT NULL, SalePrice decimal(18,2) NOT NULL, StockQuantity int NOT NULL, MinimumStockLevel int NOT NULL, IsActive bit NOT NULL,
 CONSTRAINT FK_ShopProductVariants_ShopProducts_ProductId FOREIGN KEY(ProductId) REFERENCES ShopProducts(Id) ON DELETE CASCADE,
 CONSTRAINT FK_ShopProductVariants_ShopSizes_SizeId FOREIGN KEY(SizeId) REFERENCES ShopSizes(Id),
 CONSTRAINT FK_ShopProductVariants_ShopColors_ColorId FOREIGN KEY(ColorId) REFERENCES ShopColors(Id)
);
CREATE UNIQUE INDEX IX_ShopProductVariants_SKU ON ShopProductVariants(SKU);
CREATE UNIQUE INDEX IX_ShopProductVariants_Product_Size_Color ON ShopProductVariants(ProductId,SizeId,ColorId);

IF OBJECT_ID('ShopProductImages') IS NULL
CREATE TABLE ShopProductImages (Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopProductImages PRIMARY KEY, ProductId uniqueidentifier NOT NULL, ImageUrl nvarchar(1000) NOT NULL, IsPrimary bit NOT NULL, CreatedAt datetime2 NOT NULL, CONSTRAINT FK_ShopProductImages_ShopProducts_ProductId FOREIGN KEY(ProductId) REFERENCES ShopProducts(Id) ON DELETE CASCADE);

IF OBJECT_ID('ShopCarts') IS NULL
CREATE TABLE ShopCarts (Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopCarts PRIMARY KEY, UserId uniqueidentifier NOT NULL, CreatedAt datetime2 NOT NULL, UpdatedAt datetime2 NOT NULL, CONSTRAINT FK_ShopCarts_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE);
CREATE UNIQUE INDEX IX_ShopCarts_UserId ON ShopCarts(UserId);

IF OBJECT_ID('ShopCartItems') IS NULL
CREATE TABLE ShopCartItems (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopCartItems PRIMARY KEY, CartId uniqueidentifier NOT NULL, ProductVariantId uniqueidentifier NOT NULL, Quantity int NOT NULL, UnitPrice decimal(18,2) NOT NULL,
 AddedAt datetime2 NOT NULL, UserId uniqueidentifier NOT NULL, ProductId nvarchar(max) NOT NULL, Name nvarchar(max) NOT NULL, ImageUrl nvarchar(max) NULL,
 CONSTRAINT FK_ShopCartItems_ShopCarts_CartId FOREIGN KEY(CartId) REFERENCES ShopCarts(Id) ON DELETE CASCADE,
 CONSTRAINT FK_ShopCartItems_ShopProductVariants_ProductVariantId FOREIGN KEY(ProductVariantId) REFERENCES ShopProductVariants(Id),
 CONSTRAINT FK_ShopCartItems_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id)
);
CREATE UNIQUE INDEX IX_ShopCartItems_Cart_ProductVariant ON ShopCartItems(CartId,ProductVariantId);

IF OBJECT_ID('ShopAddresses') IS NULL
CREATE TABLE ShopAddresses (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopAddresses PRIMARY KEY, UserId uniqueidentifier NOT NULL, AddressLine nvarchar(500) NOT NULL, City nvarchar(100) NOT NULL, Area nvarchar(100) NULL,
 PostalCode nvarchar(20) NULL, Country nvarchar(100) NOT NULL, IsDefault bit NOT NULL, CreatedAt datetime2 NOT NULL,
 CONSTRAINT FK_ShopAddresses_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

IF OBJECT_ID('ShopOrders') IS NULL
CREATE TABLE ShopOrders (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopOrders PRIMARY KEY, OrderNumber nvarchar(50) NOT NULL, UserId uniqueidentifier NOT NULL,
 TotalAmount decimal(18,2) NOT NULL, DiscountAmount decimal(18,2) NOT NULL, ShippingAmount decimal(18,2) NOT NULL, FinalAmount decimal(18,2) NOT NULL,
 PaymentMethod int NOT NULL, PaymentStatus int NOT NULL, OrderStatus int NOT NULL, ShippingAddressId uniqueidentifier NULL, CreatedAt datetime2 NOT NULL, UpdatedAt datetime2 NULL,
 CONSTRAINT FK_ShopOrders_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id),
 CONSTRAINT FK_ShopOrders_ShopAddresses_ShippingAddressId FOREIGN KEY(ShippingAddressId) REFERENCES ShopAddresses(Id)
);
CREATE UNIQUE INDEX IX_ShopOrders_OrderNumber ON ShopOrders(OrderNumber);

IF OBJECT_ID('ShopOrderItems') IS NULL
CREATE TABLE ShopOrderItems (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopOrderItems PRIMARY KEY, OrderId uniqueidentifier NOT NULL, ProductVariantId uniqueidentifier NOT NULL,
 ProductName nvarchar(250) NOT NULL, SKU nvarchar(100) NOT NULL, Quantity int NOT NULL, UnitPrice decimal(18,2) NOT NULL, TotalPrice decimal(18,2) NOT NULL,
 CONSTRAINT FK_ShopOrderItems_ShopOrders_OrderId FOREIGN KEY(OrderId) REFERENCES ShopOrders(Id) ON DELETE CASCADE,
 CONSTRAINT FK_ShopOrderItems_ShopProductVariants_ProductVariantId FOREIGN KEY(ProductVariantId) REFERENCES ShopProductVariants(Id)
);

IF OBJECT_ID('ShopPaymentProofs') IS NULL
CREATE TABLE ShopPaymentProofs (Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopPaymentProofs PRIMARY KEY, UserId uniqueidentifier NOT NULL, StoredFileName nvarchar(255) NOT NULL, OriginalFileName nvarchar(255) NOT NULL, ContentType nvarchar(100) NOT NULL, Size bigint NOT NULL, CreatedAt datetime2 NOT NULL, CONSTRAINT FK_ShopPaymentProofs_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE);

IF OBJECT_ID('ShopPayments') IS NULL
CREATE TABLE ShopPayments (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopPayments PRIMARY KEY, OrderId uniqueidentifier NOT NULL, PaymentMethod int NOT NULL, Amount decimal(18,2) NOT NULL,
 PaymentStatus int NOT NULL, TransactionReference nvarchar(200) NULL, Provider nvarchar(100) NULL, BankName nvarchar(150) NULL, PaymentProofUrl nvarchar(1000) NULL,
 PaidAt datetime2 NULL, CreatedAt datetime2 NOT NULL, CONSTRAINT FK_ShopPayments_ShopOrders_OrderId FOREIGN KEY(OrderId) REFERENCES ShopOrders(Id) ON DELETE CASCADE
);

IF OBJECT_ID('ShopStockTransactions') IS NULL
CREATE TABLE ShopStockTransactions (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopStockTransactions PRIMARY KEY, ProductVariantId uniqueidentifier NOT NULL, TransactionType int NOT NULL, Quantity int NOT NULL,
 ReferenceType nvarchar(max) NULL, ReferenceId uniqueidentifier NULL, Notes nvarchar(max) NULL, CreatedAt datetime2 NOT NULL, CreatedBy uniqueidentifier NULL,
 CONSTRAINT FK_ShopStockTransactions_ShopProductVariants_ProductVariantId FOREIGN KEY(ProductVariantId) REFERENCES ShopProductVariants(Id)
);

IF OBJECT_ID('ShopSuppliers') IS NULL
CREATE TABLE ShopSuppliers (Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopSuppliers PRIMARY KEY, Name nvarchar(200) NOT NULL, Phone nvarchar(max) NULL, Email nvarchar(max) NULL, Address nvarchar(max) NULL, IsActive bit NOT NULL, CreatedAt datetime2 NOT NULL);

IF OBJECT_ID('ShopPurchases') IS NULL
CREATE TABLE ShopPurchases (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopPurchases PRIMARY KEY, SupplierId uniqueidentifier NOT NULL, PurchaseNumber nvarchar(50) NOT NULL, InvoiceNumber nvarchar(max) NULL,
 PurchaseDate datetime2 NOT NULL, TotalAmount decimal(18,2) NOT NULL, PaymentStatus int NOT NULL, Notes nvarchar(max) NULL, CreatedAt datetime2 NOT NULL,
 CONSTRAINT FK_ShopPurchases_ShopSuppliers_SupplierId FOREIGN KEY(SupplierId) REFERENCES ShopSuppliers(Id)
);
CREATE UNIQUE INDEX IX_ShopPurchases_PurchaseNumber ON ShopPurchases(PurchaseNumber);

IF OBJECT_ID('ShopPurchaseItems') IS NULL
CREATE TABLE ShopPurchaseItems (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopPurchaseItems PRIMARY KEY, PurchaseId uniqueidentifier NOT NULL, ProductVariantId uniqueidentifier NOT NULL,
 Quantity int NOT NULL, PurchasePrice decimal(18,2) NOT NULL, TotalPrice decimal(18,2) NOT NULL,
 CONSTRAINT FK_ShopPurchaseItems_ShopPurchases_PurchaseId FOREIGN KEY(PurchaseId) REFERENCES ShopPurchases(Id) ON DELETE CASCADE,
 CONSTRAINT FK_ShopPurchaseItems_ShopProductVariants_ProductVariantId FOREIGN KEY(ProductVariantId) REFERENCES ShopProductVariants(Id)
);

IF OBJECT_ID('ShopInvoices') IS NULL
CREATE TABLE ShopInvoices (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopInvoices PRIMARY KEY, OrderId uniqueidentifier NOT NULL, InvoiceNumber nvarchar(50) NOT NULL, InvoiceDate datetime2 NOT NULL,
 Status nvarchar(max) NOT NULL, CreatedAt datetime2 NOT NULL, CONSTRAINT FK_ShopInvoices_ShopOrders_OrderId FOREIGN KEY(OrderId) REFERENCES ShopOrders(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_ShopInvoices_InvoiceNumber ON ShopInvoices(InvoiceNumber);

IF OBJECT_ID('ShopRefreshTokens') IS NULL
CREATE TABLE ShopRefreshTokens (
 Id uniqueidentifier NOT NULL CONSTRAINT PK_ShopRefreshTokens PRIMARY KEY, UserId uniqueidentifier NOT NULL, TokenHash nvarchar(128) NOT NULL,
 ExpiresAt datetime2 NOT NULL, CreatedAt datetime2 NOT NULL, RevokedAt datetime2 NULL,
 CONSTRAINT FK_ShopRefreshTokens_Users_UserId FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IX_ShopRefreshTokens_TokenHash ON ShopRefreshTokens(TokenHash);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP TABLE IF EXISTS ShopRefreshTokens;
DROP TABLE IF EXISTS ShopPaymentProofs;
DROP TABLE IF EXISTS ShopInvoices;
DROP TABLE IF EXISTS ShopPurchaseItems;
DROP TABLE IF EXISTS ShopPurchases;
DROP TABLE IF EXISTS ShopSuppliers;
DROP TABLE IF EXISTS ShopStockTransactions;
DROP TABLE IF EXISTS ShopPayments;
DROP TABLE IF EXISTS ShopOrderItems;
DROP TABLE IF EXISTS ShopOrders;
DROP TABLE IF EXISTS ShopAddresses;
DROP TABLE IF EXISTS ShopCartItems;
DROP TABLE IF EXISTS ShopCarts;
DROP TABLE IF EXISTS ShopProductImages;
DROP TABLE IF EXISTS ShopProductVariants;
DROP TABLE IF EXISTS ShopProducts;
DROP TABLE IF EXISTS ShopAgeGroups;
DROP TABLE IF EXISTS ShopColors;
DROP TABLE IF EXISTS ShopSizes;
DROP TABLE IF EXISTS ShopBrands;
DROP TABLE IF EXISTS ShopCategories;
DROP TABLE IF EXISTS AspNetUserTokens;
DROP TABLE IF EXISTS AspNetUserLogins;
DROP TABLE IF EXISTS AspNetUserClaims;
DROP TABLE IF EXISTS AspNetUserRoles;
DROP TABLE IF EXISTS AspNetRoles;
""");
    }
}
