using Project3D.Domain.Common;

namespace Project3D.Domain.Entities;

public class CustomRequestImage : BaseEntity
{
    public Guid CustomRequestId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? PublicId { get; set; }          // ID no Cloudinary
    public string OriginalName { get; set; } = string.Empty;

    // Navegação
    public virtual CustomRequest CustomRequest { get; set; } = null!;
}
