namespace Project3D.Application.DTOs.Payments;

public record CreatePaymentPreferenceRequest(Guid OrderId);

public record PaymentPreferenceResponse(
    string PreferenceId,
    string CheckoutUrl,         // InitPoint (produção)
    string SandboxCheckoutUrl   // SandboxInitPoint (testes)
);

public record MercadoPagoWebhookDto(
    string Action,             // "payment.created", "payment.updated"
    string ApiVersion,
    WebhookData Data,
    string Id,
    DateTimeOffset DateCreated,
    string LiveMode,
    string Type,
    string UserId
);

public record WebhookData(string Id);
