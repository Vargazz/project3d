using Project3D.Infrastructure.Data;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Project3D.Application.DTOs.Products;
using Project3D.Application.Services.Interfaces;
using Project3D.Domain.Entities;
using Project3D.Domain.Interfaces;

namespace Project3D.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;

    public ProductService(AppDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterRequest filter, CancellationToken ct = default)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Subcategory)
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search) || p.Description.Contains(filter.Search) || p.Tags!.Contains(filter.Search));

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId);

        if (filter.SubcategoryId.HasValue)
            query = query.Where(p => p.SubcategoryId == filter.SubcategoryId);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => (p.PromotionalPrice ?? p.Price) >= filter.MinPrice);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => (p.PromotionalPrice ?? p.Price) <= filter.MaxPrice);

        if (filter.InStock.HasValue && filter.InStock.Value)
            query = query.Where(p => p.Inventory != null && p.Inventory.Quantity > 0);

        if (filter.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == filter.IsFeatured);

        if (filter.IsNew.HasValue)
            query = query.Where(p => p.IsNew == filter.IsNew);

        query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
        {
            ("price", "desc") => query.OrderByDescending(p => p.PromotionalPrice ?? p.Price),
            ("price", _) => query.OrderBy(p => p.PromotionalPrice ?? p.Price),
            ("sold", _) => query.OrderByDescending(p => p.SoldCount),
            ("new", _) => query.OrderByDescending(p => p.CreatedAt),
            ("name", "desc") => query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var dtos = items.Select(MapToDto).ToList();
        return new PagedResult<ProductDto>(dtos, total, filter.Page, filter.PageSize);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await GetProductWithDetails().FirstOrDefaultAsync(p => p.Id == id, ct);
        return product is null ? null : MapToDto(product);
    }

    public async Task<ProductDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var product = await GetProductWithDetails().FirstOrDefaultAsync(p => p.Slug == slug, ct);
        return product is null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetFeaturedAsync(int limit = 8, CancellationToken ct = default)
    {
        var products = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .AsNoTracking()
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetNewestAsync(int limit = 8, CancellationToken ct = default)
    {
        var products = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .AsNoTracking()
            .Where(p => p.IsActive && p.IsNew)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetBestSellersAsync(int limit = 8, CancellationToken ct = default)
    {
        var products = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.SoldCount)
            .Take(limit)
            .ToListAsync(ct);
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetRelatedAsync(Guid productId, int limit = 4, CancellationToken ct = default)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null) return [];

        var related = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Inventory)
            .AsNoTracking()
            .Where(p => p.IsActive && p.Id != productId && p.CategoryId == product.CategoryId)
            .OrderByDescending(p => p.SoldCount)
            .Take(limit)
            .ToListAsync(ct);
        return related.Select(MapToDto);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        if (await _db.Products.AnyAsync(p => p.Sku == request.Sku, ct))
            throw new InvalidOperationException($"SKU '{request.Sku}' já existe.");

        var slug = GenerateSlug(request.Name);
        var baseSlug = slug;
        var counter = 1;
        while (await _db.Products.AnyAsync(p => p.Slug == slug, ct))
            slug = $"{baseSlug}-{counter++}";

        var product = new Product
        {
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            Sku = request.Sku,
            CategoryId = request.CategoryId,
            SubcategoryId = request.SubcategoryId,
            Price = request.Price,
            PromotionalPrice = request.PromotionalPrice,
            IsActive = request.IsActive,
            IsFeatured = request.IsFeatured,
            IsNew = request.IsNew,
            WeightGrams = request.WeightGrams,
            HeightCm = request.HeightCm,
            WidthCm = request.WidthCm,
            DepthCm = request.DepthCm,
            MetaTitle = request.MetaTitle,
            MetaDescription = request.MetaDescription,
            Tags = request.Tags
        };

        await _db.Products.AddAsync(product, ct);

        var inventory = new Inventory
        {
            ProductId = product.Id,
            Quantity = request.InitialStock,
            MinStock = request.MinStock
        };
        await _db.Inventories.AddAsync(inventory, ct);

        await _db.SaveChangesAsync(ct);

        return (await GetByIdAsync(product.Id, ct))!;
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Produto não encontrado.");

        if (await _db.Products.AnyAsync(p => p.Sku == request.Sku && p.Id != id, ct))
            throw new InvalidOperationException($"SKU '{request.Sku}' já está em uso por outro produto.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = request.Sku;
        product.CategoryId = request.CategoryId;
        product.SubcategoryId = request.SubcategoryId;
        product.Price = request.Price;
        product.PromotionalPrice = request.PromotionalPrice;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.IsNew = request.IsNew;
        product.WeightGrams = request.WeightGrams;
        product.HeightCm = request.HeightCm;
        product.WidthCm = request.WidthCm;
        product.DepthCm = request.DepthCm;
        product.MetaTitle = request.MetaTitle;
        product.MetaDescription = request.MetaDescription;
        product.Tags = request.Tags;

        await _db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Produto não encontrado.");
        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<ProductDto> ToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Produto não encontrado.");
        product.IsActive = !product.IsActive;
        await _db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<string> AddImageAsync(Guid productId, Stream stream, string fileName, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync([productId], ct)
            ?? throw new KeyNotFoundException("Produto não encontrado.");

        var result = await _storage.UploadAsync(stream, fileName, "products", ct);

        var hasPrimary = await _db.ProductImages.AnyAsync(i => i.ProductId == productId && i.IsPrimary, ct);
        var order = await _db.ProductImages.CountAsync(i => i.ProductId == productId, ct);

        var image = new ProductImage
        {
            ProductId = productId,
            Url = result.Url,
            PublicId = result.PublicId,
            AltText = product.Name,
            DisplayOrder = order,
            IsPrimary = !hasPrimary
        };

        await _db.ProductImages.AddAsync(image, ct);
        await _db.SaveChangesAsync(ct);

        return result.Url;
    }

    public async Task RemoveImageAsync(Guid productId, Guid imageId, CancellationToken ct = default)
    {
        var image = await _db.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId, ct)
            ?? throw new KeyNotFoundException("Imagem não encontrada.");

        if (image.PublicId is not null)
            await _storage.DeleteAsync(image.PublicId, ct);

        _db.ProductImages.Remove(image);

        // Se era a primária, promover a próxima
        if (image.IsPrimary)
        {
            var next = await _db.ProductImages
                .Where(i => i.ProductId == productId && i.Id != imageId)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefaultAsync(ct);
            if (next is not null) next.IsPrimary = true;
        }

        await _db.SaveChangesAsync(ct);
    }

    // ---- Privado ----

    private IQueryable<Product> GetProductWithDetails()
        => _db.Products
            .Include(p => p.Category)
            .Include(p => p.Subcategory)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.CustomizationFields)
            .Include(p => p.Inventory)
            .AsNoTracking();

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        Description = p.Description,
        Sku = p.Sku,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        SubcategoryId = p.SubcategoryId,
        SubcategoryName = p.Subcategory?.Name,
        Price = p.Price,
        PromotionalPrice = p.PromotionalPrice,
        IsActive = p.IsActive,
        IsFeatured = p.IsFeatured,
        IsNew = p.IsNew,
        SoldCount = p.SoldCount,
        StockQuantity = p.Inventory?.Quantity,
        Tags = p.Tags,
        MetaTitle = p.MetaTitle,
        MetaDescription = p.MetaDescription,
        WeightGrams = p.WeightGrams,
        CreatedAt = p.CreatedAt,
        PrimaryImageUrl = p.Images
            .Where(i => i.IsPrimary)
            .Select(i => i.Url)
            .FirstOrDefault()
            ?? p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).FirstOrDefault(),
        Images = p.Images.OrderBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto(i.Id, i.Url, i.AltText, i.DisplayOrder, i.IsPrimary))
            .ToList(),
        Variants = p.Variants.Where(v => v.IsActive)
            .Select(v => new ProductVariantDto(v.Id, v.Name, v.Value, v.Type, v.PriceModifier, v.IsActive))
            .ToList(),
        CustomizationFields = p.CustomizationFields.OrderBy(f => f.DisplayOrder)
            .Select(f => new ProductCustomizationFieldDto(f.Id, f.Label, f.FieldType, f.IsRequired, f.Options, f.DisplayOrder))
            .ToList()
    };

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[àáâãäå]", "a");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[èéêë]", "e");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[ìíîï]", "i");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[òóôõö]", "o");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[ùúûü]", "u");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[ç]", "c");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');
        return slug;
    }
}
