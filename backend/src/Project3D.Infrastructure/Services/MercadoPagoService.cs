using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Project3D.Domain.Interfaces;

namespace Project3D.Infrastructure.Services;

/// <summary>
/// Serviço de integração com Mercado Pago.
/// Utiliza Checkout Pro (redirect para página do MP).
/// </summary>
public class MercadoPagoService : IPaymentService
{
    private readonly IConfiguration _config;
    private readonly ILogger<MercadoPagoService> _logger;

    public MercadoPagoService(IConfiguration config, ILogger<MercadoPagoService> logger)
    {
        _config = config;
        _logger = logger;

        var accessToken = config["MercadoPago:AccessToken"];
        if (!string.IsNullOrWhiteSpace(accessToken))
            MercadoPagoConfig.AccessToken = accessToken;
    }

    public async Task<CreatePreferenceResult> CreatePreferenceAsync(
        CreatePreferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var notificationUrl = _config["MercadoPago:NotificationUrl"];
        var successUrl = _config["MercadoPago:SuccessUrl"];
        var failureUrl = _config["MercadoPago:FailureUrl"];
        var pendingUrl = _config["MercadoPago:PendingUrl"];

        var preferenceRequest = new PreferenceRequest
        {
            Items =
            [
                new PreferenceItemRequest
                {
                    Title = request.Title,
                    Quantity = 1,
                    UnitPrice = request.Amount,
                    CurrencyId = "BRL"
                }
            ],
            Payer = new PreferencePayerRequest
            {
                Name = request.PayerName,
                Email = request.PayerEmail
            },
            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = successUrl,
                Failure = failureUrl,
                Pending = pendingUrl
            },
            // AutoReturn = "approved", // Não usar — status via webhook apenas
            NotificationUrl = notificationUrl,
            ExternalReference = request.OrderReference,
            StatementDescriptor = "PRINT3D STUDIO",
            Expires = false
        };

        var client = new PreferenceClient();
        Preference preference = await client.CreateAsync(preferenceRequest, cancellationToken: cancellationToken);

        _logger.LogInformation("Preferência MP criada: {PreferenceId} para pedido {OrderRef}",
            preference.Id, request.OrderReference);

        return new CreatePreferenceResult(
            preference.Id,
            preference.InitPoint,
            preference.SandboxInitPoint
        );
    }

    public async Task<PaymentInfo?> GetPaymentInfoAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new MercadoPago.Client.Payment.PaymentClient();
            var payment = await client.GetAsync(long.Parse(paymentId), cancellationToken: cancellationToken);

            if (payment is null) return null;

            return new PaymentInfo(
                payment.Id.ToString()!,
                payment.Status ?? "unknown",
                payment.StatusDetail ?? string.Empty,
                payment.TransactionAmount ?? 0,
                payment.PaymentTypeId,
                payment.DateApproved?.ToUniversalTime()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pagamento MP: {PaymentId}", paymentId);
            return null;
        }
    }
}
