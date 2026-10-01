using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project3D.Application.DTOs.Payments;
using Project3D.Application.Services.Interfaces;

namespace Project3D.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentProcessingService _paymentService;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
    private readonly Microsoft.Extensions.Logging.ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentProcessingService paymentService,
        Microsoft.Extensions.Configuration.IConfiguration config,
        Microsoft.Extensions.Logging.ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _config = config;
        _logger = logger;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Cria uma preferência de pagamento Mercado Pago para um pedido.
    /// Retorna a URL do Checkout Pro.
    /// </summary>
    [Authorize]
    [HttpPost("orders/{orderId:guid}/preference")]
    public async Task<IActionResult> CreateOrderPreference(Guid orderId, CancellationToken ct)
    {
        var result = await _paymentService.CreateOrderPreferenceAsync(orderId, UserId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Cria uma preferência de pagamento para um orçamento personalizado aceito.
    /// </summary>
    [Authorize]
    [HttpPost("quotes/{quoteId:guid}/preference")]
    public async Task<IActionResult> CreateQuotePreference(Guid quoteId, CancellationToken ct)
    {
        var result = await _paymentService.CreateQuotePreferenceAsync(quoteId, UserId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Webhook do Mercado Pago.
    /// IMPORTANTE: O status do pagamento NUNCA é atualizado pelo redirect — somente por aqui.
    /// O MP envia uma notificação POST para esta URL ao confirmar o pagamento.
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        // Ler body raw para validação de assinatura futura
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        _logger.LogInformation("Webhook MP recebido: {Body}", body);

        // Obter o ID do pagamento a partir dos query params (notification_type=payment) 
        // ou do body (IPN vs Webhooks v2)
        string? paymentId = null;

        // Formato webhook v2 (corpo JSON)
        if (Request.ContentType?.Contains("application/json") == true)
        {
            try
            {
                var dto = System.Text.Json.JsonSerializer.Deserialize<MercadoPagoWebhookDto>(body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dto?.Type == "payment" && dto.Data?.Id is not null)
                    paymentId = dto.Data.Id;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao desserializar webhook MP.");
            }
        }

        // Formato IPN (query string)
        paymentId ??= Request.Query["data.id"].FirstOrDefault()
                   ?? Request.Query["id"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(paymentId))
        {
            // Processar de forma assíncrona sem bloquear a resposta
            _ = Task.Run(async () =>
            {
                try { await _paymentService.ProcessWebhookAsync(paymentId, CancellationToken.None); }
                catch (Exception ex) { _logger.LogError(ex, "Erro ao processar webhook pagamento {Id}", paymentId); }
            }, ct);
        }

        // Responder 200 imediatamente para o MP não retentar
        return Ok();
    }
}
