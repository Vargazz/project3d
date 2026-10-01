using Microsoft.AspNetCore.Identity;
using Project3D.Domain.Enums;

namespace Project3D.Infrastructure.Identity;

/// <summary>
/// Usuário da aplicação — combina ASP.NET Identity com campos de domínio.
/// Esta é a entidade usada pelo EF Core e Identity Manager.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegação (EF Core)
    public virtual ICollection<Domain.Entities.Address> Addresses { get; set; } = [];
    public virtual ICollection<Domain.Entities.Order> Orders { get; set; } = [];
    public virtual ICollection<Domain.Entities.CustomRequest> CustomRequests { get; set; } = [];
    public virtual ICollection<Domain.Entities.Notification> Notifications { get; set; } = [];
}
