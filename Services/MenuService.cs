using CLOTHS_ERP.API.Dtos;
using CLOTHS_ERP.API.Interfaces;
using CLOTHS_ERP.API.Options;
using Microsoft.Extensions.Options;

namespace CLOTHS_ERP.API.Services;

public class MenuService : IMenuService
{
    private const string AdminRole = "Admin";
    private const string UserRole = "User";

    private const string CartWishNode = "Carts/Wish";

    private readonly IMenuRepository _menuRepository;
    private readonly MenuOptions _options;

    public MenuService(
        IMenuRepository menuRepository,
        IOptions<MenuOptions> options)
    {
        _menuRepository = menuRepository;
        _options = options.Value;
    }

    public async Task<List<MenuNodeDto>> GetMenuAsync(
        int pcId,
        string role,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return new List<MenuNodeDto>();
        }

        var normalizedRole =
            role.Trim();

        var isAdmin =
            IsAdministrator(normalizedRole);

        var isUser =
            normalizedRole.Equals(
                UserRole,
                StringComparison.OrdinalIgnoreCase);

        if (!isAdmin && !isUser)
        {
            return new List<MenuNodeDto>();
        }

        // Admin ka actual PC_ID 0 ho sakta hai.
        // SP ko configured admin PC_ID diya jayega.
        var effectivePcId =
            isAdmin && pcId == 0
                ? _options.AdminPcId
                : pcId;

        var databaseMenu =
            await _menuRepository.GetMenuAsync(
                effectivePcId,
                cancellationToken);

        if (isAdmin)
        {
            return BuildAdminMenu(databaseMenu);
        }

        return BuildUserMenu(databaseMenu);
    }

    // =========================================================
    // ADMIN
    // =========================================================

    private static List<MenuNodeDto> BuildAdminMenu(
        List<MenuNodeDto> databaseMenu)
    {
        var result = databaseMenu
            .Where(n => !n.Label.Equals(CartWishNode, StringComparison.OrdinalIgnoreCase))
            .ToList();

        result.Add(BuildShopNode());
        result.Add(BuildOrdersNode());

        return result;
    }

    // =========================================================
    // USER
    // =========================================================

    private static List<MenuNodeDto> BuildUserMenu(
        List<MenuNodeDto> databaseMenu)
    {
        // Customer menu: user can browse and purchase, but no admin/setup nodes.
        return new List<MenuNodeDto>
        {
            BuildShopNode(),
            BuildCartNode(),
            BuildOrdersNode(),
            BuildProfileNode()
        };
    }

    // =========================================================
    // SHOP
    // =========================================================

    private static MenuNodeDto BuildShopNode()
    {
        return new MenuNodeDto
        {
            Id = 9001,
            Label = "Shop",
            Icon = "bi-shop",
            Children =
            [
                new MenuNodeDto
                {
                    Id = 9101,
                    FormId = 9101,
                    Label = "All Products",
                    Route = "/app/shop"
                },

                new MenuNodeDto
                {
                    Id = 9102,
                    FormId = 9102,
                    Label = "Men's Wear",
                    Route = "/app/shop/men"
                },

                new MenuNodeDto
                {
                    Id = 9103,
                    FormId = 9103,
                    Label = "Women's Wear",
                    Route = "/app/shop/women"
                },

                new MenuNodeDto
                {
                    Id = 9104,
                    FormId = 9104,
                    Label = "Kids' Wear",
                    Route = "/app/shop/kids"
                }
            ]
        };
    }

    private static MenuNodeDto BuildCartNode() => new()
    {
        Id = 9003,
        Label = "Cart",
        Icon = "bi-cart3",
        Route = "/app/cart"
    };

    private static MenuNodeDto BuildProfileNode() => new()
    {
        Id = 9004,
        Label = "My Profile",
        Icon = "bi-person",
        Route = "/app/profile"
    };

    // =========================================================
    // ORDERS
    // =========================================================

    private static MenuNodeDto BuildOrdersNode()
    {
        return new MenuNodeDto
        {
            Id = 9002,
            Label = "My Orders",
            Icon = "bi-receipt",
            Route = "/app/orders"
        };
    }

    // =========================================================
    // ROLE CHECK
    // =========================================================

    private static bool IsAdministrator(
        string role)
    {
        return
            role.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase)
            ||
            role.Equals(
                "Administrator",
                StringComparison.OrdinalIgnoreCase)
            ||
            role.Equals(
                "System Administrator",
                StringComparison.OrdinalIgnoreCase);
    }
}