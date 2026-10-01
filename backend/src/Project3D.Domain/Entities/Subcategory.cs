using Project3D.Domain.Common;

namespace Project3D.Domain.Entities;

public class Subcategory : BaseEntity
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navegação
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Product> Products { get; set; } = [];
}
