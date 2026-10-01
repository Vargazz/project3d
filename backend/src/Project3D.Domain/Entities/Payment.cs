using Project3D.Domain.Common;
using Project3D.Domain.Enums;

namespace Project3D.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid? CustomQuoteId { get; set; }

    public string Provider { get; set; } = "MERCADO_PAGO";
    public string? ExternalId { get; set; }        // ID do pagamento no Mercado Pago
    public string? PreferenceId { get; set; }      // ID da preferência criada
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? Method { get; set; }            // pix, credit_card, debit_card
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? RawResponse { get; set; }       // JSON da resposta completa do MP

    // Navegação
    public virtual Order? Order { get; set; }
    public virtual CustomQuote? CustomQuote { get; set; }
}
