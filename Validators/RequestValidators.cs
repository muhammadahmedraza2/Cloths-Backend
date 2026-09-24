using ClothingErp.Api.Dtos;
using FluentValidation;

namespace ClothingErp.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class ProductRequestValidator : AbstractValidator<ProductRequestDto>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Variants).SetValidator(new ProductVariantRequestValidator());
    }
}

public class ProductVariantRequestValidator : AbstractValidator<ProductVariantRequestDto>
{
    public ProductVariantRequestValidator()
    {
        RuleFor(x => x.SizeId).NotEmpty();
        RuleFor(x => x.ColorId).NotEmpty();
        RuleFor(x => x.SKU).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinimumStockLevel).GreaterThanOrEqualTo(0);
    }
}

public class AddToCartValidator : AbstractValidator<AddToCartRequestDto>
{
    public AddToCartValidator() { RuleFor(x => x.ProductVariantId).NotEmpty(); RuleFor(x => x.Quantity).GreaterThan(0); }
}

public class CreateOrderValidator : AbstractValidator<CreateOrderRequestDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ShippingAddressId).NotEmpty();
        RuleFor(x => x.ShippingAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}
