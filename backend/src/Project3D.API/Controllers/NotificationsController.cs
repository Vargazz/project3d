using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project3D.Domain.Interfaces;
using Project3D.Infrastructure.Data;

namespace Project3D.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public NotificationsController(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        var query = _db.Notifications
            .Where(n => n.UserId == UserId)
            .AsNoTracking();

        if (unreadOnly) query = query.Where(n => !n.IsRead);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new
            {
                n.Id, n.Title, n.Message, n.Type, n.IsRead,
                n.ReferenceType, n.ReferenceId, n.CreatedAt
            })
            .ToListAsync(ct);

        var unreadCount = await _db.Notifications
            .CountAsync(n => n.UserId == UserId && !n.IsRead, ct);

        return Ok(new { notifications, unreadCount });
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct)
    {
        await _notificationService.MarkAsReadAsync(id, UserId, ct);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == UserId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in notifications) n.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
