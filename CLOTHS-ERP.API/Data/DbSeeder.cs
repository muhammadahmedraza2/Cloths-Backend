using System.Text.Json;
using ClothingErp.Api.Interfaces;
using ClothingErp.Api.Models;

namespace ClothingErp.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db, IPasswordHasher hasher)
    {
        //db.Database.EnsureCreated();

        if (!db.Users.Any())
        {
            db.Users.Add(new AppUser
            {
                Username = "admin",
                PasswordHash = hasher.Hash("Admin@123"),
                FullName = "System Administrator",
                Role = "Administrator"
            });
            db.SaveChanges();
        }

        if (db.FormDefinitions.Any())
        {
            return; // already seeded
        }

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

        db.SaveChanges();
    }
}