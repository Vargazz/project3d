using Project3D.Application.DTOs.CustomRequests;
using Project3D.Application.DTOs.Products;

namespace Project3D.Application.Services.Interfaces;

public interface ICustomRequestService
{
    Task<CustomRequestDto> CreateAsync(Guid userId, CreateCustomRequestRequest request, CancellationToken ct = default);
    Task<CustomRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<CustomRequestDto>> GetUserRequestsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<PagedResult<CustomRequestDto>> GetAllRequestsAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken ct = default);
    Task<CustomRequestDto> UpdateStatusAsync(Guid id, UpdateCustomRequestStatusRequest request, Guid adminId, CancellationToken ct = default);
    Task<string> AddImageAsync(Guid requestId, Stream stream, string fileName, CancellationToken ct = default);
    Task<CustomQuoteDto> CreateQuoteAsync(Guid requestId, CreateQuoteRequest request, Guid adminId, CancellationToken ct = default);
    Task<CustomQuoteDto> RespondToQuoteAsync(Guid quoteId, Guid userId, RespondQuoteRequest request, CancellationToken ct = default);
}
