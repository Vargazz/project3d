using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project3D.Application.DTOs.CustomRequests;
using Project3D.Application.Services.Interfaces;

namespace Project3D.API.Controllers;

[ApiController]
[Route("api/custom-requests")]
public class CustomRequestsController : ControllerBase
{
    private readonly ICustomRequestService _service;

    public CustomRequestsController(ICustomRequestService service) => _service = service;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomRequestRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(UserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await _service.GetUserRequestsAsync(UserId, page, pageSize, ct));

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        if (result is null) return NotFound();
        if (!User.IsInRole("Admin") && result.UserId != UserId) return Forbid();
        return Ok(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddImage(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest(new { message = "Arquivo inválido." });
        await using var stream = file.OpenReadStream();
        var url = await _service.AddImageAsync(id, stream, file.FileName, ct);
        return Ok(new { url });
    }

    [Authorize]
    [HttpPost("{id:guid}/quotes/{quoteId:guid}/respond")]
    public async Task<IActionResult> RespondToQuote(Guid id, Guid quoteId, [FromBody] RespondQuoteRequest request, CancellationToken ct)
        => Ok(await _service.RespondToQuoteAsync(quoteId, UserId, request, ct));

    // ---- Admin ----

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
        => Ok(await _service.GetAllRequestsAsync(page, pageSize, status, ct));

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCustomRequestStatusRequest request, CancellationToken ct)
        => Ok(await _service.UpdateStatusAsync(id, request, UserId, ct));

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/quotes")]
    public async Task<IActionResult> CreateQuote(Guid id, [FromBody] CreateQuoteRequest request, CancellationToken ct)
        => Ok(await _service.CreateQuoteAsync(id, request, UserId, ct));
}
