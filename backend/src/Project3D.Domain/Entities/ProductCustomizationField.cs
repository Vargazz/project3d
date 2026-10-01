using Project3D.Domain.Common;
using Project3D.Domain.Enums;

namespace Project3D.Domain.Entities;

/// <summary>
/// Campos de personalização de um produto (ex: nome a imprimir, cor, texto).
/// </summary>
public class ProductCustomizationField : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Label { get; set; } = string.Empty;     // Ex: "Nome personalizado"
    public CustomFieldType FieldType { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? Options { get; set; }                  // JSON array de opções para campos Select
    public int DisplayOrder { get; set; } = 0;

    // Navegação
    public virtual Product Product { get; set; } = null!;
}
