using Project3D.Domain.Common;
using Project3D.Domain.Enums;

namespace Project3D.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public string? ReferenceType { get; set; }   // "Order", "CustomRequest", "Quote"
    public Guid? ReferenceId { get; set; }

    // Navegação (sem referência direta ao User — controlada pela Infrastructure)
}
