using Project3D.Domain.Common;

namespace Project3D.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navegação
    public virtual ICollection<Subcategory> Subcategories { get; set; } = [];
    public virtual ICollection<Product> Products { get; set; } = [];
}
