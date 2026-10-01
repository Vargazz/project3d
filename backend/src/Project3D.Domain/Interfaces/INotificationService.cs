namespace Project3D.Domain.Interfaces;

/// <summary>
/// Serviço de notificações internas.
/// Estruturado para futuramente suportar e-mail, WhatsApp e SMS.
/// </summary>
public interface INotificationService
{
    Task SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
}

public record SendNotificationRequest(
    Guid UserId,
    string Title,
    string Message,
    Domain.Enums.NotificationType Type,
    string? ReferenceType = null,
    Guid? ReferenceId = null
);
