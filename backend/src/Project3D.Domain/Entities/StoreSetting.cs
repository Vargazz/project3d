using Project3D.Domain.Common;

namespace Project3D.Domain.Entities;

/// <summary>
/// Configurações gerais da loja (chave-valor).
/// Permite configurar nome, logo, contatos, redes sociais, políticas, etc.
/// </summary>
public class StoreSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
}
