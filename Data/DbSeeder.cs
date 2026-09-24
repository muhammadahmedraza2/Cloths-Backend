using System.Text.Json;
using ClothingErp.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<AppUser> userManager,
        IConfiguration configuration)
    {
        await db.Database.MigrateAsync();

        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }

        var adminUsername = configuration["SeedAdmin:Username"] ?? "admin";
        var adminPassword = configuration["SeedAdmin:Password"] ?? Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD");

        var legacyUsers = await db.Users.Where(x => x.NormalizedUserName == null).ToListAsync();
        foreach (var legacyUser in legacyUsers)
        {
            legacyUser.NormalizedUserName = (legacyUser.UserName ?? "").ToUpperInvariant();
            legacyUser.NormalizedEmail = legacyUser.Email?.ToUpperInvariant();
            legacyUser.SecurityStamp ??= Guid.NewGuid().ToString();
            legacyUser.ConcurrencyStamp ??= Guid.NewGuid().ToString();
            legacyUser.IsActive = true;
        }
        if (legacyUsers.Count > 0)
            await db.SaveChangesAsync();

        var existingUsers = await db.Users.ToListAsync();
        foreach (var existingUser in existingUsers)
        {
            var role = existingUser.Role.Equals("Administrator", StringComparison.OrdinalIgnoreCase) ||
                       existingUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? "Admin"
                : "User";

            if (!await userManager.IsInRoleAsync(existingUser, role))
                await userManager.AddToRoleAsync(existingUser, role);
        }

        if (!string.IsNullOrWhiteSpace(adminPassword))
        {
            var admin = await userManager.FindByNameAsync(adminUsername);
            if (admin is null)
            {
                admin = new AppUser
                {
                    Id = Guid.NewGuid(),
                    UserName = adminUsername,
                    FullName = "System Administrator",
                    Role = "Admin",
                    PcId = configuration.GetValue<int>("Menu:AdminPcId", 5000),
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
            else
            {
                admin.Role = "Admin";
                admin.IsActive = true;
                admin.PcId = configuration.GetValue<int>("Menu:AdminPcId", 5000);
                if (string.IsNullOrWhiteSpace(admin.SecurityStamp))
                    admin.SecurityStamp = Guid.NewGuid().ToString();

                var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
                var reset = await userManager.ResetPasswordAsync(admin, resetToken, adminPassword);
                if (!reset.Succeeded)
                    throw new InvalidOperationException(string.Join(" ", reset.Errors.Select(x => x.Description)));

                await userManager.UpdateAsync(admin);
                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Frock", Description = "Girls frocks" },
                new Category { Name = "Romper", Description = "Baby rompers" },
                new Category { Name = "Bodysuit", Description = "Baby bodysuits" },
                new Category { Name = "T-Shirt", Description = "Kids t-shirts" },
                new Category { Name = "Shirt", Description = "Kids shirts" },
                new Category { Name = "Trouser", Description = "Kids trousers" },
                new Category { Name = "Jeans", Description = "Kids jeans" },
                new Category { Name = "Kurta Pajama", Description = "Traditional kids wear" },
                new Category { Name = "Shalwar Kameez", Description = "Traditional kids wear" },
                new Category { Name = "Maxi", Description = "Girls maxi" },
                new Category { Name = "Lehenga", Description = "Girls festive wear" },
                new Category { Name = "Gharara", Description = "Girls festive wear" },
                new Category { Name = "Sharara", Description = "Girls festive wear" },
                new Category { Name = "Night Suit", Description = "Kids night wear" },
                new Category { Name = "Tracksuit", Description = "Kids tracksuits" },
                new Category { Name = "Hoodie", Description = "Kids hoodies" },
                new Category { Name = "Sweater", Description = "Kids sweaters" },
                new Category { Name = "Jacket", Description = "Kids jackets" });
        }

        if (!await db.Sizes.AnyAsync())
            db.Sizes.AddRange(
                new Size { Name = "0-1 Years", AgeRange = "0-1" },
                new Size { Name = "1-2 Years", AgeRange = "1-2" },
                new Size { Name = "2-3 Years", AgeRange = "2-3" },
                new Size { Name = "3-4 Years", AgeRange = "3-4" },
                new Size { Name = "4-5 Years", AgeRange = "4-5" },
                new Size { Name = "5-6 Years", AgeRange = "5-6" },
                new Size { Name = "6-8 Years", AgeRange = "6-8" },
                new Size { Name = "8-10 Years", AgeRange = "8-10" },
                new Size { Name = "10-12 Years", AgeRange = "10-12" });

        if (!await db.Colors.AnyAsync())
            db.Colors.AddRange(
                new Color { Name = "Pink", HexCode = "#FFC0CB" },
                new Color { Name = "Blue", HexCode = "#0000FF" },
                new Color { Name = "White", HexCode = "#FFFFFF" },
                new Color { Name = "Black", HexCode = "#000000" },
                new Color { Name = "Red", HexCode = "#FF0000" });

        if (!await db.AgeGroups.AnyAsync())
            db.AgeGroups.AddRange(
                new AgeGroup { Name = "Baby", MinAgeMonths = 0, MaxAgeMonths = 24 },
                new AgeGroup { Name = "Toddler", MinAgeMonths = 25, MaxAgeMonths = 60 },
                new AgeGroup { Name = "Kids", MinAgeMonths = 61, MaxAgeMonths = 144 });

        if (!await db.Brands.AnyAsync())
            db.Brands.AddRange(new Brand { Name = "Little Threads" }, new Brand { Name = "Kids Collection" });

        await db.SaveChangesAsync();

        var cols = (string[] keys, string[] labels) =>
            JsonSerializer.Serialize(keys.Zip(labels, (k, l) => new { key = k, label = l }));

        var forms = new List<(FormDefinition Form, List<(string status, string closed, Dictionary<string, object?> data)> Rows)>
        {
            (new FormDefinition
            {
                Id = 1047,
                Title = "Stationery Setup",
                Breadcrumb = "Setup Management / Product Setup / Stationery Setup",
                ColumnsJson = cols(
                    new[] { "code", "name", "instrumentId", "instrumentDesc" },
                    new[] { "Stationery Code", "Stationery Name", "Instrument Id", "INSTRUMENT_DESC" })
            }, new()
            {
                ("UnAuthorize", "N", new() { ["code"] = "03123", ["name"] = "RAZA STATIONARY SETUP", ["instrumentId"] = "MK17", ["instrumentDesc"] = "MK17" }),
                ("Authorized", "N", new() { ["code"] = "", ["name"] = "", ["instrumentId"] = "", ["instrumentDesc"] = "" }),
                ("UnAuthorize", "Y", new() { ["code"] = "12345", ["name"] = "Shoaib", ["instrumentId"] = "MK17", ["instrumentDesc"] = "MK17" }),
                ("UnAuthorize", "Y", new() { ["code"] = "ts01", ["name"] = "testing", ["instrumentId"] = "MK17", ["instrumentDesc"] = "MK17" }),
                ("Authorized", "N", new() { ["code"] = "SC_KK", ["name"] = "ayan", ["instrumentId"] = "", ["instrumentDesc"] = "" }),
            }),

            (new FormDefinition
            {
                Id = 1101,
                Title = "Company Setup",
                Breadcrumb = "Setup Management / Company Setup",
                ColumnsJson = cols(new[] { "code", "name", "city", "ntn" }, new[] { "Company Code", "Company Name", "City", "NTN" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "CMP01", ["name"] = "CRPL Textiles Pvt Ltd", ["city"] = "Karachi", ["ntn"] = "1234567-8" }),
                ("UnAuthorize", "N", new() { ["code"] = "CMP02", ["name"] = "Little Threads (Pvt) Ltd", ["city"] = "Lahore", ["ntn"] = "9876543-1" }),
            }),

            (new FormDefinition
            {
                Id = 1102,
                Title = "Product Setup",
                Breadcrumb = "Setup Management / Product Setup",
                ColumnsJson = cols(new[] { "code", "name", "category", "uom", "price", "imageUrl" },
                                   new[] { "Product Code", "Product Name", "Category", "UOM", "Price (Rs.)", "Image URL" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "PRD001", ["name"] = "Kids Winter Jacket", ["category"] = "Kids Wear", ["uom"] = "PCS", ["price"] = 3500, ["imageUrl"] = "" }),
                ("Authorized", "N", new() { ["code"] = "PRD002", ["name"] = "Men's Formal Shirt", ["category"] = "Men Wear", ["uom"] = "PCS", ["price"] = 2200, ["imageUrl"] = "" }),
                ("UnAuthorize", "N", new() { ["code"] = "PRD003", ["name"] = "Women's Lawn Suit", ["category"] = "Women Wear", ["uom"] = "SET", ["price"] = 4800, ["imageUrl"] = "" }),
                ("UnAuthorize", "N", new() { ["code"] = "PRD004", ["name"] = "Newborn Romper", ["category"] = "Kids Wear", ["uom"] = "PCS", ["price"] = 1500, ["imageUrl"] = "" }),
            }),

            (new FormDefinition
            {
                Id = 1103,
                Title = "Bank Setup",
                Breadcrumb = "Setup Management / Bank Setup",
                ColumnsJson = cols(new[] { "code", "name", "branch", "account" }, new[] { "Bank Code", "Bank Name", "Branch", "Account No" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "BNK01", ["name"] = "Meezan Bank", ["branch"] = "Gulshan-e-Iqbal", ["account"] = "01234567890" }),
                ("UnAuthorize", "N", new() { ["code"] = "BNK02", ["name"] = "HBL", ["branch"] = "Defence", ["account"] = "09876543210" }),
            }),

            (new FormDefinition
            {
                Id = 1104,
                Title = "Geographical Setup",
                Breadcrumb = "Setup Management / Geographical Setup",
                ColumnsJson = cols(new[] { "code", "name", "country" }, new[] { "Region Code", "Region / City", "Country" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "KHI", ["name"] = "Karachi", ["country"] = "Pakistan" }),
                ("Authorized", "N", new() { ["code"] = "LHR", ["name"] = "Lahore", ["country"] = "Pakistan" }),
                ("UnAuthorize", "N", new() { ["code"] = "ISB", ["name"] = "Islamabad", ["country"] = "Pakistan" }),
            }),

            (new FormDefinition
            {
                Id = 1105,
                Title = "Signatory Management",
                Breadcrumb = "Setup Management / Signatory Management",
                ColumnsJson = cols(new[] { "code", "name", "designation" }, new[] { "Signatory Code", "Signatory Name", "Designation" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "SIG01", ["name"] = "Ahmed Raza", ["designation"] = "Finance Manager" }),
                ("UnAuthorize", "N", new() { ["code"] = "SIG02", ["name"] = "Ayesha Khan", ["designation"] = "Operations Head" }),
            }),

            (new FormDefinition
            {
                Id = 2001,
                Title = "Sales Order",
                Breadcrumb = "Sales Management / Sales Order",
                ColumnsJson = cols(new[] { "code", "name", "amount", "date", "imageUrl" }, new[] { "Order No", "Customer Name", "Amount (Rs.)", "Order Date", "Image URL" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "SO-1001", ["name"] = "Ali Garments Outlet", ["amount"] = "45,000", ["date"] = "12-Sep-2026", ["imageUrl"] = "" }),
                ("UnAuthorize", "N", new() { ["code"] = "SO-1002", ["name"] = "Fatima Kids Store", ["amount"] = "18,500", ["date"] = "15-Sep-2026", ["imageUrl"] = "" }),
            }),

            (new FormDefinition
            {
                Id = 3001,
                Title = "Purchase Order",
                Breadcrumb = "Purchase Management / Purchase Order",
                ColumnsJson = cols(new[] { "code", "name", "amount" }, new[] { "PO No", "Supplier Name", "Amount (Rs.)" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "PO-501", ["name"] = "Al-Karam Textile Mills", ["amount"] = "2,10,000" }),
            }),

            (new FormDefinition
            {
                Id = 4001,
                Title = "Stock Overview",
                Breadcrumb = "Inventory Management / Stock Overview",
                ColumnsJson = cols(new[] { "code", "name", "qty", "warehouse" }, new[] { "Product Code", "Product Name", "Qty in Stock", "Warehouse" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "PRD001", ["name"] = "Kids Winter Jacket", ["qty"] = "240", ["warehouse"] = "Main Warehouse" }),
                ("Authorized", "N", new() { ["code"] = "PRD003", ["name"] = "Women's Lawn Suit", ["qty"] = "85", ["warehouse"] = "Main Warehouse" }),
            }),

            (new FormDefinition
            {
                Id = 5002,
                Title = "Supplier Ledger",
                Breadcrumb = "Payment/Payable / Supplier Ledger",
                ColumnsJson = cols(new[] { "code", "name", "amount" }, new[] { "Supplier Code", "Supplier Name", "Balance (Rs.)" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "SUP01", ["name"] = "Al-Karam Textile Mills", ["amount"] = "2,10,000" }),
            }),

            (new FormDefinition
            {
                Id = 6002,
                Title = "Customer Ledger",
                Breadcrumb = "Receipt-Receivable / Customer Ledger",
                ColumnsJson = cols(new[] { "code", "name", "amount" }, new[] { "Customer Code", "Customer Name", "Balance (Rs.)" })
            }, new()
            {
                ("Authorized", "N", new() { ["code"] = "CUS01", ["name"] = "Ali Garments Outlet", ["amount"] = "45,000" }),
            }),
        };

        foreach (var (form, rows) in forms)
        {
            db.FormDefinitions.Add(form);
            foreach (var (status, closed, data) in rows)
            {
                db.MasterRecords.Add(new MasterRecord
                {
                    FormId = form.Id,
                    Status = status,
                    Closed = closed,
                    DataJson = JsonSerializer.Serialize(data),
                    CreatedBy = "seed"
                });
            }
        }


        await db.SaveChangesAsync();
    }
}
