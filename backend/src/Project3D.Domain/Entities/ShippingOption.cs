using Project3D.Domain.Common;

namespace Project3D.Domain.Entities;

public class ShippingOption : BaseEntity
{
    public string Name { get; set; } = string.Empty;          // Ex: "PAC", "SEDEX", "Retirada"
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int EstimatedDays { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
}
