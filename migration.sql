BEGIN TRANSACTION;
GO

ALTER TABLE [CartItems] DROP CONSTRAINT [FK_CartItems_Users_UserId];
GO

ALTER TABLE [OrderItems] DROP CONSTRAINT [FK_OrderItems_Orders_OrderId];
GO

ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Users_UserId];
GO

ALTER TABLE [Orders] DROP CONSTRAINT [PK_Orders];
GO

ALTER TABLE [OrderItems] DROP CONSTRAINT [PK_OrderItems];
GO

ALTER TABLE [CartItems] DROP CONSTRAINT [PK_CartItems];
GO

EXEC sp_rename N'[Orders]', N'ShopOrders';
GO

EXEC sp_rename N'[OrderItems]', N'ShopOrderItems';
GO

EXEC sp_rename N'[CartItems]', N'ShopCartItems';
GO

EXEC sp_rename N'[ShopOrders].[OrderNo]', N'OrderNumber', N'COLUMN';
GO

EXEC sp_rename N'[ShopOrders].[IX_Orders_UserId]', N'IX_ShopOrders_UserId', N'INDEX';
GO

EXEC sp_rename N'[ShopOrderItems].[IX_OrderItems_OrderId]', N'IX_ShopOrderItems_OrderId', N'INDEX';
GO

EXEC sp_rename N'[ShopCartItems].[IX_CartItems_UserId]', N'IX_ShopCartItems_UserId', N'INDEX';
GO

ALTER TABLE [Users] ADD [AccessFailedCount] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Users] ADD [ConcurrencyStamp] nvarchar(max) NULL;
GO

ALTER TABLE [Users] ADD [Email] nvarchar(256) NULL;
GO

ALTER TABLE [Users] ADD [EmailConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Users] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Users] ADD [LockoutEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Users] ADD [LockoutEnd] datetimeoffset NULL;
GO

ALTER TABLE [Users] ADD [NormalizedEmail] nvarchar(256) NULL;
GO

ALTER TABLE [Users] ADD [NormalizedUserName] nvarchar(256) NULL;
GO

ALTER TABLE [Users] ADD [PhoneNumber] nvarchar(50) NULL;
GO

ALTER TABLE [Users] ADD [PhoneNumberConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [Users] ADD [SecurityStamp] nvarchar(max) NULL;
GO

ALTER TABLE [Users] ADD [TwoFactorEnabled] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [ShopOrders] ADD [DiscountAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [ShopOrders] ADD [FinalAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [ShopOrders] ADD [OrderStatus] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [ShopOrders] ADD [PaymentStatus] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [ShopOrders] ADD [ShippingAddressId] uniqueidentifier NULL;
GO

ALTER TABLE [ShopOrders] ADD [ShippingAmount] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [ShopOrders] ADD [UpdatedAt] datetime2 NULL;
GO

ALTER TABLE [ShopOrderItems] ADD [ProductName] nvarchar(250) NOT NULL DEFAULT N'';
GO

ALTER TABLE [ShopOrderItems] ADD [ProductVariantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
GO

ALTER TABLE [ShopOrderItems] ADD [Quantity] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [ShopOrderItems] ADD [SKU] nvarchar(100) NOT NULL DEFAULT N'';
GO

ALTER TABLE [ShopOrderItems] ADD [TotalPrice] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [ShopOrderItems] ADD [UnitPrice] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [ShopCartItems] ADD [CartId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
GO

ALTER TABLE [ShopCartItems] ADD [ProductVariantId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
GO

ALTER TABLE [ShopCartItems] ADD [Quantity] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [ShopCartItems] ADD [UnitPrice] decimal(18,2) NOT NULL DEFAULT 0.0;
GO

ALTER TABLE [ShopOrders] ADD CONSTRAINT [PK_ShopOrders] PRIMARY KEY ([Id]);
GO

ALTER TABLE [ShopOrderItems] ADD CONSTRAINT [PK_ShopOrderItems] PRIMARY KEY ([Id]);
GO

ALTER TABLE [ShopCartItems] ADD CONSTRAINT [PK_ShopCartItems] PRIMARY KEY ([Id]);
GO

CREATE TABLE [AspNetRoles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] uniqueidentifier NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopAddresses] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [AddressLine] nvarchar(500) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Area] nvarchar(100) NULL,
    [PostalCode] nvarchar(20) NULL,
    [Country] nvarchar(100) NOT NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ShopAddresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopAddresses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopAgeGroups] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [MinAgeMonths] int NULL,
    [MaxAgeMonths] int NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ShopAgeGroups] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ShopBrands] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ShopBrands] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ShopCarts] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ShopCarts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopCarts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopCategories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Description] nvarchar(max) NULL,
    [ImageUrl] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ShopCategories] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ShopColors] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    [HexCode] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ShopColors] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ShopDepartments] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ShopDepartments] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ShopInvoices] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [InvoiceNumber] nvarchar(50) NOT NULL,
    [InvoiceDate] datetime2 NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ShopInvoices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopInvoices_ShopOrders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [ShopOrders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopPaymentProofs] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [StoredFileName] nvarchar(255) NOT NULL,
    [OriginalFileName] nvarchar(255) NOT NULL,
    [ContentType] nvarchar(100) NOT NULL,
    [Size] bigint NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ShopPaymentProofs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopPaymentProofs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopPayments] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NOT NULL,
    [PaymentMethod] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [PaymentStatus] int NOT NULL,
    [TransactionReference] nvarchar(200) NULL,
    [Provider] nvarchar(100) NULL,
    [BankName] nvarchar(150) NULL,
    [PaymentProofUrl] nvarchar(1000) NULL,
    [PaidAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ShopPayments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopPayments_ShopOrders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [ShopOrders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopRefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [TokenHash] nvarchar(128) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [RevokedAt] datetime2 NULL,
    CONSTRAINT [PK_ShopRefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopRefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopSetTypes] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [PieceCount] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ShopSetTypes] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ShopSizes] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(80) NOT NULL,
    [AgeRange] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ShopSizes] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [ShopSuppliers] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ShopSuppliers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] uniqueidentifier NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopProducts] (
    [Id] uniqueidentifier NOT NULL,
    [ProductName] nvarchar(250) NOT NULL,
    [SKU] nvarchar(100) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CategoryId] uniqueidentifier NOT NULL,
    [BrandId] uniqueidentifier NULL,
    [Gender] int NOT NULL,
    [AgeGroupId] uniqueidentifier NULL,
    [DepartmentId] uniqueidentifier NULL,
    [SetTypeId] uniqueidentifier NULL,
    [SetIncludes] nvarchar(300) NULL,
    [Fabric] nvarchar(max) NULL,
    [Season] nvarchar(max) NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [SalePrice] decimal(18,2) NOT NULL,
    [Discount] decimal(18,2) NOT NULL,
    [StockQuantity] int NOT NULL,
    [MinimumStockLevel] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_ShopProducts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopProducts_ShopAgeGroups_AgeGroupId] FOREIGN KEY ([AgeGroupId]) REFERENCES [ShopAgeGroups] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_ShopProducts_ShopBrands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [ShopBrands] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_ShopProducts_ShopCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [ShopCategories] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ShopProducts_ShopDepartments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [ShopDepartments] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_ShopProducts_ShopSetTypes_SetTypeId] FOREIGN KEY ([SetTypeId]) REFERENCES [ShopSetTypes] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [ShopPurchases] (
    [Id] uniqueidentifier NOT NULL,
    [SupplierId] uniqueidentifier NOT NULL,
    [PurchaseNumber] nvarchar(50) NOT NULL,
    [InvoiceNumber] nvarchar(max) NULL,
    [PurchaseDate] datetime2 NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [PaymentStatus] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ShopPurchases] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopPurchases_ShopSuppliers_SupplierId] FOREIGN KEY ([SupplierId]) REFERENCES [ShopSuppliers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ShopProductImages] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ImageUrl] nvarchar(1000) NOT NULL,
    [IsPrimary] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ShopProductImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopProductImages_ShopProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [ShopProducts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopProductVariants] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [SizeId] uniqueidentifier NOT NULL,
    [ColorId] uniqueidentifier NOT NULL,
    [SKU] nvarchar(100) NOT NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [SalePrice] decimal(18,2) NOT NULL,
    [StockQuantity] int NOT NULL,
    [MinimumStockLevel] int NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ShopProductVariants] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopProductVariants_ShopColors_ColorId] FOREIGN KEY ([ColorId]) REFERENCES [ShopColors] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ShopProductVariants_ShopProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [ShopProducts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ShopProductVariants_ShopSizes_SizeId] FOREIGN KEY ([SizeId]) REFERENCES [ShopSizes] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ShopPurchaseItems] (
    [Id] uniqueidentifier NOT NULL,
    [PurchaseId] uniqueidentifier NOT NULL,
    [ProductVariantId] uniqueidentifier NOT NULL,
    [Quantity] int NOT NULL,
    [PurchasePrice] decimal(18,2) NOT NULL,
    [TotalPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ShopPurchaseItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopPurchaseItems_ShopProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ShopProductVariants] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ShopPurchaseItems_ShopPurchases_PurchaseId] FOREIGN KEY ([PurchaseId]) REFERENCES [ShopPurchases] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [ShopStockTransactions] (
    [Id] uniqueidentifier NOT NULL,
    [ProductVariantId] uniqueidentifier NOT NULL,
    [TransactionType] int NOT NULL,
    [Quantity] int NOT NULL,
    [ReferenceType] nvarchar(max) NULL,
    [ReferenceId] uniqueidentifier NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] uniqueidentifier NULL,
    CONSTRAINT [PK_ShopStockTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShopStockTransactions_ShopProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ShopProductVariants] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [EmailIndex] ON [Users] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_ShopOrders_OrderNumber] ON [ShopOrders] ([OrderNumber]);
GO

CREATE INDEX [IX_ShopOrders_ShippingAddressId] ON [ShopOrders] ([ShippingAddressId]);
GO

CREATE INDEX [IX_ShopOrderItems_ProductVariantId] ON [ShopOrderItems] ([ProductVariantId]);
GO

CREATE UNIQUE INDEX [IX_ShopCartItems_CartId_ProductVariantId] ON [ShopCartItems] ([CartId], [ProductVariantId]);
GO

CREATE INDEX [IX_ShopCartItems_ProductVariantId] ON [ShopCartItems] ([ProductVariantId]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [IX_ShopAddresses_UserId] ON [ShopAddresses] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_ShopAgeGroups_Name] ON [ShopAgeGroups] ([Name]);
GO

CREATE UNIQUE INDEX [IX_ShopBrands_Name] ON [ShopBrands] ([Name]);
GO

CREATE UNIQUE INDEX [IX_ShopCarts_UserId] ON [ShopCarts] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_ShopCategories_Name] ON [ShopCategories] ([Name]);
GO

CREATE UNIQUE INDEX [IX_ShopColors_Name] ON [ShopColors] ([Name]);
GO

CREATE UNIQUE INDEX [IX_ShopDepartments_Name] ON [ShopDepartments] ([Name]);
GO

CREATE UNIQUE INDEX [IX_ShopInvoices_InvoiceNumber] ON [ShopInvoices] ([InvoiceNumber]);
GO

CREATE UNIQUE INDEX [IX_ShopInvoices_OrderId] ON [ShopInvoices] ([OrderId]);
GO

CREATE INDEX [IX_ShopPaymentProofs_UserId] ON [ShopPaymentProofs] ([UserId]);
GO

CREATE INDEX [IX_ShopPayments_OrderId] ON [ShopPayments] ([OrderId]);
GO

CREATE INDEX [IX_ShopProductImages_ProductId] ON [ShopProductImages] ([ProductId]);
GO

CREATE INDEX [IX_ShopProducts_AgeGroupId] ON [ShopProducts] ([AgeGroupId]);
GO

CREATE INDEX [IX_ShopProducts_BrandId] ON [ShopProducts] ([BrandId]);
GO

CREATE INDEX [IX_ShopProducts_CategoryId] ON [ShopProducts] ([CategoryId]);
GO

CREATE INDEX [IX_ShopProducts_DepartmentId] ON [ShopProducts] ([DepartmentId]);
GO

CREATE INDEX [IX_ShopProducts_SetTypeId] ON [ShopProducts] ([SetTypeId]);
GO

CREATE UNIQUE INDEX [IX_ShopProducts_SKU] ON [ShopProducts] ([SKU]);
GO

CREATE INDEX [IX_ShopProductVariants_ColorId] ON [ShopProductVariants] ([ColorId]);
GO

CREATE UNIQUE INDEX [IX_ShopProductVariants_ProductId_SizeId_ColorId] ON [ShopProductVariants] ([ProductId], [SizeId], [ColorId]);
GO

CREATE INDEX [IX_ShopProductVariants_SizeId] ON [ShopProductVariants] ([SizeId]);
GO

CREATE UNIQUE INDEX [IX_ShopProductVariants_SKU] ON [ShopProductVariants] ([SKU]);
GO

CREATE INDEX [IX_ShopPurchaseItems_ProductVariantId] ON [ShopPurchaseItems] ([ProductVariantId]);
GO

CREATE INDEX [IX_ShopPurchaseItems_PurchaseId] ON [ShopPurchaseItems] ([PurchaseId]);
GO

CREATE UNIQUE INDEX [IX_ShopPurchases_PurchaseNumber] ON [ShopPurchases] ([PurchaseNumber]);
GO

CREATE INDEX [IX_ShopPurchases_SupplierId] ON [ShopPurchases] ([SupplierId]);
GO

CREATE UNIQUE INDEX [IX_ShopRefreshTokens_TokenHash] ON [ShopRefreshTokens] ([TokenHash]);
GO

CREATE INDEX [IX_ShopRefreshTokens_UserId] ON [ShopRefreshTokens] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_ShopSetTypes_Name] ON [ShopSetTypes] ([Name]);
GO

CREATE UNIQUE INDEX [IX_ShopSizes_Name] ON [ShopSizes] ([Name]);
GO

CREATE INDEX [IX_ShopStockTransactions_ProductVariantId] ON [ShopStockTransactions] ([ProductVariantId]);
GO

CREATE UNIQUE INDEX [IX_ShopSuppliers_Name] ON [ShopSuppliers] ([Name]);
GO

ALTER TABLE [ShopCartItems] ADD CONSTRAINT [FK_ShopCartItems_ShopCarts_CartId] FOREIGN KEY ([CartId]) REFERENCES [ShopCarts] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [ShopCartItems] ADD CONSTRAINT [FK_ShopCartItems_ShopProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ShopProductVariants] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [ShopCartItems] ADD CONSTRAINT [FK_ShopCartItems_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]);
GO

ALTER TABLE [ShopOrderItems] ADD CONSTRAINT [FK_ShopOrderItems_ShopOrders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [ShopOrders] ([Id]) ON DELETE CASCADE;
GO

ALTER TABLE [ShopOrderItems] ADD CONSTRAINT [FK_ShopOrderItems_ShopProductVariants_ProductVariantId] FOREIGN KEY ([ProductVariantId]) REFERENCES [ShopProductVariants] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [ShopOrders] ADD CONSTRAINT [FK_ShopOrders_ShopAddresses_ShippingAddressId] FOREIGN KEY ([ShippingAddressId]) REFERENCES [ShopAddresses] ([Id]) ON DELETE SET NULL;
GO

ALTER TABLE [ShopOrders] ADD CONSTRAINT [FK_ShopOrders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260926092308_AddDepartmentSetTypeToCatalog', N'8.0.20');
GO

COMMIT;
GO

