using Project3D.Domain.Common;

namespace Project3D.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? PublicId { get; set; }   // ID no Cloudinary (quando ativo)
    public string? AltText { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;

    // Navegação
    public virtual Product Product { get; set; } = null!;
}
