using Project3D.Domain.Common;

namespace Project3D.Domain.Entities;

/// <summary>
/// Log de operações administrativas para rastreabilidade.
/// </summary>
public class AdminLog : BaseEntity
{
    public Guid AdminId { get; set; }
    public string Action { get; set; } = string.Empty;        // Ex: "UPDATE_ORDER_STATUS"
    public string Entity { get; set; } = string.Empty;        // Ex: "Order"
    public Guid? EntityId { get; set; }
    public string? Details { get; set; }                      // JSON com detalhes da operação
    public string? IpAddress { get; set; }
}
