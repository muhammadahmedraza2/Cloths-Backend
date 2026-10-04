using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class CatalogService
{
    private readonly AppDbContext _db;

    public CatalogService(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<Product> ProductQuery()
    {
        return _db.Products
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
                .ThenInclude(x => x.Color);
    }

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
        var query = ProductQuery().Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            query = query.Where(x =>
                x.ProductName.Contains(value) ||
                x.SKU.Contains(value));
        }

        if (departmentId.HasValue)
            query = query.Where(x => x.DepartmentId == departmentId.Value);

        if (categoryId.HasValue)
            query = query.Where(x => x.CategoryId == categoryId.Value);

        if (gender.HasValue)
            query = query.Where(x => x.Gender == gender.Value);

        if (ageGroupId.HasValue)
            query = query.Where(x => x.AgeGroupId == ageGroupId.Value);

        if (sizeId.HasValue)
            query = query.Where(x => x.Variants.Any(v =>
                v.SizeId == sizeId.Value && v.IsActive));

        if (colorId.HasValue)
            query = query.Where(x => x.Variants.Any(v =>
                v.ColorId == colorId.Value && v.IsActive));

        if (setTypeId.HasValue)
            query = query.Where(x => x.SetTypeId == setTypeId.Value);

        var products = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var result = products.Select(ProductMapper.MapProduct).ToList();

        return new CatalogProductsResultDto
        {
            Found = result.Count > 0,
            Message = result.Count > 0 ? null : "NOT FOUND",
            Products = result
        };
    }

    public async Task<ProductResponseDto?> GetProductAsync(Guid id)
    {
        var product = await ProductQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

        return product is null ? null : ProductMapper.MapProduct(product);
    }

    public Task<List<Category>> GetCategoriesAsync() =>
        _db.Categories.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

    public Task<List<Brand>> GetBrandsAsync() =>
        _db.Brands.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

    public Task<List<Size>> GetSizesAsync() =>
        _db.Sizes.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

    public Task<List<Color>> GetColorsAsync() =>
        _db.Colors.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

    public Task<List<AgeGroup>> GetAgeGroupsAsync() =>
        _db.AgeGroups.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.MinAgeMonths)
            .ToListAsync();

    public Task<List<Department>> GetDepartmentsAsync() =>
        _db.Departments.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();

    public Task<List<SetType>> GetSetTypesAsync() =>
        _db.SetTypes.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.PieceCount)
            .ThenBy(x => x.Name)
            .ToListAsync();
}