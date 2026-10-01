using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project3D.Application.Services.Interfaces;

namespace Project3D.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IDashboardService _dashboard;
    private readonly IInventoryService _inventory;
    private readonly IUserService _users;
    private readonly ICouponService _coupons;
    private readonly IStoreSettingService _settings;
    private readonly IAdminUserService _adminUsers;

    public AdminController(
        IDashboardService dashboard,
        IInventoryService inventory,
        IUserService users,
        ICouponService coupons,
        IStoreSettingService settings,
        IAdminUserService adminUsers)
    {
        _dashboard = dashboard;
        _inventory = inventory;
        _users = users;
        _coupons = coupons;
        _settings = settings;
        _adminUsers = adminUsers;
    }

    // ---- Dashboard ----

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
        => Ok(await _dashboard.GetSummaryAsync(ct));

    // ---- Estoque ----

    [HttpGet("inventory/alerts")]
    public async Task<IActionResult> GetLowStockAlerts(CancellationToken ct)
        => Ok(await _inventory.GetLowStockAlertsAsync(ct));

    [HttpPost("inventory/adjust")]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        var adminId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        await _inventory.AdjustStockAsync(
            request.ProductId, request.VariantId, request.Quantity,
            request.MovementType, request.Reason, null, adminId, ct);
        return NoContent();
    }

    // ---- Clientes ----

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await _users.GetAllAsync(page, pageSize, ct));

    [HttpPatch("users/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleUser(Guid id, CancellationToken ct)
    {
        await _users.ToggleActiveAsync(id, ct);
        return NoContent();
    }

    // ---- Administradores ----

    [HttpGet("admins")]
    public async Task<IActionResult> GetAdmins(CancellationToken ct)
        => Ok(await _adminUsers.GetAllAsync(ct));

    [HttpPost("admins")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request, CancellationToken ct)
    {
        try
        {
            var admin = await _adminUsers.CreateAsync(request, ct);
            return Ok(admin);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("admins/{id:guid}")]
    public async Task<IActionResult> DeleteAdmin(Guid id, CancellationToken ct)
    {
        var currentId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        if (id == currentId) return BadRequest(new { message = "Você não pode excluir sua própria conta." });
        try
        {
            await _adminUsers.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("admins/{id:guid}/reset-password")]
    public async Task<IActionResult> ResetAdminPassword(Guid id, [FromBody] ResetAdminPasswordRequest request, CancellationToken ct)
    {
        try
        {
            await _adminUsers.ResetPasswordAsync(id, request.NewPassword, ct);
            return NoContent();
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ---- Cupons ----

    [HttpGet("coupons")]
    public async Task<IActionResult> GetCoupons(CancellationToken ct)
        => Ok(await _coupons.GetAllAsync(ct));

    [HttpPost("coupons")]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request, CancellationToken ct)
        => Ok(await _coupons.CreateAsync(request, ct));

    [HttpPut("coupons/{id:guid}")]
    public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] CreateCouponRequest request, CancellationToken ct)
        => Ok(await _coupons.UpdateAsync(id, request, ct));

    [HttpDelete("coupons/{id:guid}")]
    public async Task<IActionResult> DeleteCoupon(Guid id, CancellationToken ct)
    {
        await _coupons.DeleteAsync(id, ct);
        return NoContent();
    }

    // ---- Configurações da loja ----

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
        => Ok(await _settings.GetAllAsync(ct));

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, string> settings, CancellationToken ct)
    {
        await _settings.SetBulkAsync(settings, ct);
        return NoContent();
    }
}

public record AdjustStockRequest(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    Domain.Enums.InventoryMovementType MovementType,
    string? Reason
);

public record ResetAdminPasswordRequest(string NewPassword);
