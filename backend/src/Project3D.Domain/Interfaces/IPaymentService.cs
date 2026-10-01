namespace Project3D.Domain.Interfaces;

/// <summary>
/// Contrato do serviço de pagamento.
/// Implementado com Mercado Pago SDK.
/// </summary>
public interface IPaymentService
{
    Task<CreatePreferenceResult> CreatePreferenceAsync(CreatePreferenceRequest request, CancellationToken cancellationToken = default);
    Task<PaymentInfo?> GetPaymentInfoAsync(string paymentId, CancellationToken cancellationToken = default);
}

public record CreatePreferenceRequest(
    string OrderReference,
    string Title,
    decimal Amount,
    string PayerEmail,
    string PayerName
);

public record CreatePreferenceResult(
    string PreferenceId,
    string InitPoint,       // URL do Checkout Pro
    string SandboxInitPoint
);

public record PaymentInfo(
    string Id,
    string Status,          // "approved", "pending", "rejected", etc.
    string StatusDetail,
    decimal Amount,
    string? PaymentMethod,
    DateTime? DateApproved
);
