using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project3D.Application.DTOs.Users;
using Project3D.Application.Services.Interfaces;
using Project3D.Domain.Entities;
using Project3D.Infrastructure.Data;

namespace Project3D.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly AppDbContext _db;

    public UsersController(IUserService userService, AppDbContext db)
    {
        _userService = userService;
        _db = db;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
        => Ok(await _userService.GetByIdAsync(UserId, ct));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
        => Ok(await _userService.UpdateProfileAsync(UserId, request, ct));

    // ---- Endereços ----

    [HttpGet("me/addresses")]
    public async Task<IActionResult> GetAddresses(CancellationToken ct)
    {
        var addresses = await _db.Addresses
            .Where(a => a.UserId == UserId)
            .AsNoTracking()
            .ToListAsync(ct);
        return Ok(addresses);
    }

    [HttpPost("me/addresses")]
    public async Task<IActionResult> AddAddress([FromBody] AddAddressRequest request, CancellationToken ct)
    {
        if (request.IsDefault)
        {
            var existing = await _db.Addresses.Where(a => a.UserId == UserId).ToListAsync(ct);
            foreach (var a in existing) a.IsDefault = false;
        }

        var address = new Address
        {
            UserId = UserId,
            Label = request.Label,
            ZipCode = request.ZipCode,
            State = request.State,
            City = request.City,
            Neighborhood = request.Neighborhood,
            Street = request.Street,
            Number = request.Number,
            Complement = request.Complement,
            IsDefault = request.IsDefault
        };

        await _db.Addresses.AddAsync(address, ct);
        await _db.SaveChangesAsync(ct);
        return Ok(address);
    }

    [HttpDelete("me/addresses/{id:guid}")]
    public async Task<IActionResult> RemoveAddress(Guid id, CancellationToken ct)
    {
        var address = await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId, ct);
        if (address is null) return NotFound();
        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record AddAddressRequest(
    string Label,
    string ZipCode,
    string State,
    string City,
    string Neighborhood,
    string Street,
    string Number,
    string? Complement,
    bool IsDefault = false
);
