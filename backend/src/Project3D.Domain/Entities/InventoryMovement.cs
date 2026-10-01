using Project3D.Domain.Common;
using Project3D.Domain.Enums;

namespace Project3D.Domain.Entities;

public class InventoryMovement : BaseEntity
{
    public Guid InventoryId { get; set; }
    public InventoryMovementType MovementType { get; set; }
    public int Quantity { get; set; }               // Positivo = entrada, negativo = saída
    public string? Reason { get; set; }
    public Guid? ReferenceId { get; set; }          // ID do pedido ou ajuste relacionado
    public Guid? CreatedById { get; set; }          // Usuário que realizou o movimento

    // Navegação
    public virtual Inventory Inventory { get; set; } = null!;
}
