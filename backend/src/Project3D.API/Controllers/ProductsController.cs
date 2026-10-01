using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project3D.Application.DTOs.Products;
using Project3D.Application.Services.Interfaces;

namespace Project3D.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _products;

    public ProductsController(IProductService products)
    {
        _products = products;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductFilterRequest filter, CancellationToken ct)
    {
        var result = await _products.GetProductsAsync(filter, ct);
        return Ok(result);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured([FromQuery] int limit = 8, CancellationToken ct = default)
        => Ok(await _products.GetFeaturedAsync(limit, ct));

    [HttpGet("newest")]
    public async Task<IActionResult> GetNewest([FromQuery] int limit = 8, CancellationToken ct = default)
        => Ok(await _products.GetNewestAsync(limit, ct));

    [HttpGet("best-sellers")]
    public async Task<IActionResult> GetBestSellers([FromQuery] int limit = 8, CancellationToken ct = default)
        => Ok(await _products.GetBestSellersAsync(limit, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var product = await _products.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var product = await _products.GetBySlugAsync(slug, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("{id:guid}/related")]
    public async Task<IActionResult> GetRelated(Guid id, [FromQuery] int limit = 4, CancellationToken ct = default)
        => Ok(await _products.GetRelatedAsync(id, limit, ct));

    // ---- Admin ----

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await _products.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
        => Ok(await _products.UpdateAsync(id, request, ct));

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _products.DeleteAsync(id, ct);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/toggle")]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
        => Ok(await _products.ToggleActiveAsync(id, ct));

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/images")]
    public async Task<IActionResult> AddImage(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Arquivo inválido." });

        await using var stream = file.OpenReadStream();
        var url = await _products.AddImageAsync(id, stream, file.FileName, ct);
        return Ok(new { url });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> RemoveImage(Guid id, Guid imageId, CancellationToken ct)
    {
        await _products.RemoveImageAsync(id, imageId, ct);
        return NoContent();
    }
}
