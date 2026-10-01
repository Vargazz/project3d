using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project3D.Domain.Entities;
using Project3D.Domain.Interfaces;
using Project3D.Infrastructure.Data;

namespace Project3D.Infrastructure.Services;

/// <summary>
/// Serviço de notificações — implementação inicial com persistência no banco.
/// Estruturado para adicionar e-mail/WhatsApp/SMS futuramente.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = request.UserId,
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            ReferenceType = request.ReferenceType,
            ReferenceId = request.ReferenceId,
            IsRead = false
        };

        await _context.Notifications.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Notificação enviada para usuário {UserId}: {Title}", request.UserId, request.Title);

        // TODO: Aqui serão adicionados e-mail, WhatsApp e SMS futuramente.
        // Exemplo: await _emailService.SendAsync(...)
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notification is null) return;

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
