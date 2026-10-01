using Project3D.Domain.Enums;

namespace Project3D.Application.DTOs.Products;

// ---- Listagem / exibição ----

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? SubcategoryId { get; set; }
    public string? SubcategoryName { get; set; }
    public decimal Price { get; set; }
    public decimal? PromotionalPrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public int SoldCount { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<ProductImageDto> Images { get; set; } = [];
    public List<ProductVariantDto> Variants { get; set; } = [];
    public List<ProductCustomizationFieldDto> CustomizationFields { get; set; } = [];
    public int? StockQuantity { get; set; }
    public string? Tags { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public decimal? WeightGrams { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record ProductImageDto(Guid Id, string Url, string? AltText, int DisplayOrder, bool IsPrimary);
public record ProductVariantDto(Guid Id, string Name, string Value, ProductVariantType Type, decimal PriceModifier, bool IsActive);
public record ProductCustomizationFieldDto(Guid Id, string Label, CustomFieldType FieldType, bool IsRequired, string? Options, int DisplayOrder);

// ---- Criação / edição (admin) ----

public record CreateProductRequest(
    string Name,
    string Description,
    string Sku,
    Guid CategoryId,
    Guid? SubcategoryId,
    decimal Price,
    decimal? PromotionalPrice,
    bool IsActive,
    bool IsFeatured,
    bool IsNew,
    decimal? WeightGrams,
    decimal? HeightCm,
    decimal? WidthCm,
    decimal? DepthCm,
    string? MetaTitle,
    string? MetaDescription,
    string? Tags,
    int InitialStock = 0,
    int MinStock = 5
);

public record UpdateProductRequest(
    string Name,
    string Description,
    string Sku,
    Guid CategoryId,
    Guid? SubcategoryId,
    decimal Price,
    decimal? PromotionalPrice,
    bool IsActive,
    bool IsFeatured,
    bool IsNew,
    decimal? WeightGrams,
    decimal? HeightCm,
    decimal? WidthCm,
    decimal? DepthCm,
    string? MetaTitle,
    string? MetaDescription,
    string? Tags
);

// ---- Filtros ----

public record ProductFilterRequest(
    string? Search,
    Guid? CategoryId,
    Guid? SubcategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? InStock,
    bool? IsFeatured,
    bool? IsNew,
    string SortBy = "name",
    string SortDirection = "asc",
    int Page = 1,
    int PageSize = 12
);

public record PagedResult<T>(IEnumerable<T> Items, int TotalItems, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
