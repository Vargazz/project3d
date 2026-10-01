using Project3D.Application.DTOs.Products;

namespace Project3D.Application.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductFilterRequest filter, CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IEnumerable<ProductDto>> GetFeaturedAsync(int limit = 8, CancellationToken ct = default);
    Task<IEnumerable<ProductDto>> GetNewestAsync(int limit = 8, CancellationToken ct = default);
    Task<IEnumerable<ProductDto>> GetBestSellersAsync(int limit = 8, CancellationToken ct = default);
    Task<IEnumerable<ProductDto>> GetRelatedAsync(Guid productId, int limit = 4, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> ToggleActiveAsync(Guid id, CancellationToken ct = default);
    Task<string> AddImageAsync(Guid productId, Stream stream, string fileName, CancellationToken ct = default);
    Task RemoveImageAsync(Guid productId, Guid imageId, CancellationToken ct = default);
}
