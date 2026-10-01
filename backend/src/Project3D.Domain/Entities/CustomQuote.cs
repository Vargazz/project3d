using Project3D.Domain.Common;
using Project3D.Domain.Enums;

namespace Project3D.Domain.Entities;

public class CustomQuote : BaseEntity
{
    public Guid CustomRequestId { get; set; }
    public decimal Price { get; set; }
    public int DeadlineDays { get; set; }
    public string? Description { get; set; }
    public string? AdminNotes { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.Draft;
    public DateTime? SentAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public Guid CreatedById { get; set; }          // Admin que criou

    // Navegação
    public virtual CustomRequest CustomRequest { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = [];
}
