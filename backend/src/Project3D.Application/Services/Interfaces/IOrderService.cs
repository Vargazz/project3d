using Project3D.Application.DTOs.Orders;
using Project3D.Application.DTOs.Products;

namespace Project3D.Application.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(Guid userId, CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken ct = default);
    Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, Guid adminId, CancellationToken ct = default);
    Task CancelAsync(Guid id, Guid userId, CancellationToken ct = default);
}
