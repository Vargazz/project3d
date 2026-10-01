using Project3D.Domain.Common;
using Project3D.Domain.Enums;

namespace Project3D.Domain.Entities;

public class CustomRequest : BaseEntity
{
    public string RequestNumber { get; set; } = string.Empty;  // Ex: "PERS-20240001"
    public Guid UserId { get; set; }

    public string ProductType { get; set; } = string.Empty;    // Ex: "Funko Pop"
    public string Description { get; set; } = string.Empty;
    public string? Characteristics { get; set; }
    public string? DesiredColors { get; set; }
    public string? ApproximateSize { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? EstimatedBudget { get; set; }
    public string? DesiredDeadline { get; set; }
    public string? Observations { get; set; }

    // Dados de contato (preenchidos se não estiver logado)
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactWhatsApp { get; set; }

    public CustomRequestStatus Status { get; set; } = CustomRequestStatus.Received;

    // Navegação (sem referência direta ao User — controlada pela Infrastructure)
    public virtual ICollection<CustomRequestImage> Images { get; set; } = [];
    public virtual ICollection<CustomQuote> Quotes { get; set; } = [];
}
