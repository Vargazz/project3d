using Project3D.Application.DTOs.Payments;
using Project3D.Application.DTOs.Products;
using Project3D.Domain.Entities;
using Project3D.Domain.Enums;

namespace Project3D.Application.Services.Interfaces;

public interface IPaymentProcessingService
{
    Task<PaymentPreferenceResponse> CreateOrderPreferenceAsync(Guid orderId, Guid userId, CancellationToken ct = default);
    Task<PaymentPreferenceResponse> CreateQuotePreferenceAsync(Guid quoteId, Guid userId, CancellationToken ct = default);
    Task ProcessWebhookAsync(string paymentId, CancellationToken ct = default);
}

public interface IInventoryService
{
    Task<int> GetStockAsync(Guid productId, Guid? variantId = null, CancellationToken ct = default);
    Task AdjustStockAsync(Guid productId, Guid? variantId, int quantity, InventoryMovementType type, string? reason = null, Guid? referenceId = null, Guid? adminId = null, CancellationToken ct = default);
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(CancellationToken ct = default);
}

public record LowStockAlertDto(Guid ProductId, string ProductName, string Sku, int CurrentStock, int MinStock);

public interface ICouponService
{
    Task<CouponValidationResult> ValidateAsync(string code, decimal orderTotal, CancellationToken ct = default);
    Task<Coupon> CreateAsync(CreateCouponRequest request, CancellationToken ct = default);
    Task<Coupon> UpdateAsync(Guid id, CreateCouponRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken ct = default);
}

public record CouponValidationResult(bool IsValid, string? ErrorMessage, decimal DiscountAmount, Guid? CouponId);

public record CreateCouponRequest(
    string Code,
    CouponType Type,
    decimal Value,
    decimal? MinOrderValue,
    int? MaxUses,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    bool IsActive
);

public interface IUserService
{
    Task<DTOs.Users.UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DTOs.Users.UserDto> UpdateProfileAsync(Guid id, DTOs.Users.UpdateProfileRequest request, CancellationToken ct = default);
    Task<PagedResult<DTOs.Users.UserDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task ToggleActiveAsync(Guid id, CancellationToken ct = default);
}

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default);
}

public record DashboardSummaryDto(
    decimal TotalRevenue,
    decimal RevenueThisMonth,
    int PendingOrders,
    int InProductionOrders,
    int ShippedOrders,
    int PendingCustomRequests,
    int LowStockProducts,
    int OutOfStockProducts,
    IEnumerable<TopProductDto> TopProducts,
    IEnumerable<SalesByDayDto> SalesByDay,
    IEnumerable<OrdersByStatusDto> OrdersByStatus
);

public record TopProductDto(Guid ProductId, string ProductName, int SoldCount, decimal Revenue);
public record SalesByDayDto(DateTime Date, decimal Amount, int OrderCount);
public record OrdersByStatusDto(string Status, int Count);

public interface IAdminUserService
{
    Task<IEnumerable<AdminUserDto>> GetAllAsync(CancellationToken ct = default);
    Task<AdminUserDto> CreateAsync(CreateAdminRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken ct = default);
}

public record AdminUserDto(Guid Id, string FullName, string Email, bool IsActive, DateTime CreatedAt);
public record CreateAdminRequest(string FullName, string Email, string Password);

public interface IStoreSettingService
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, CancellationToken ct = default);
    Task<Dictionary<string, string?>> GetAllAsync(CancellationToken ct = default);
    Task SetBulkAsync(Dictionary<string, string> settings, CancellationToken ct = default);
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

public record CategoryDto(Guid Id, string Name, string Slug, string? Description, string? ImageUrl, bool IsActive, int DisplayOrder, List<SubcategoryDto> Subcategories);
public record SubcategoryDto(Guid Id, Guid CategoryId, string Name, string Slug, bool IsActive);
public record CreateCategoryRequest(string Name, string? Description, bool IsActive, int DisplayOrder);
public record CreateSubcategoryRequest(Guid CategoryId, string Name, bool IsActive);
