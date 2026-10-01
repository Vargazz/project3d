using Project3D.Domain.Common;

namespace Project3D.Domain.Entities;

public class Inventory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }    // Null = estoque do produto base
    public int Quantity { get; set; } = 0;
    public int MinStock { get; set; } = 5;  // Estoque mínimo para alerta

    // Navegação
    public virtual Product Product { get; set; } = null!;
    public virtual ProductVariant? Variant { get; set; }
    public virtual ICollection<InventoryMovement> Movements { get; set; } = [];
}
