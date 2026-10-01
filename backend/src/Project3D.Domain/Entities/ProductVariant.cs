using Project3D.Domain.Common;
using Project3D.Domain.Enums;

namespace Project3D.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;      // Ex: "Cor"
    public ProductVariantType Type { get; set; }
    public string Value { get; set; } = string.Empty;     // Ex: "Vermelho"
    public decimal PriceModifier { get; set; } = 0;       // Acréscimo/desconto em relação ao preço base
    public bool IsActive { get; set; } = true;

    // Navegação
    public virtual Product Product { get; set; } = null!;
    public virtual Inventory? Inventory { get; set; }
}
