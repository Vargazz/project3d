using Project3D.Domain.Enums;

namespace Project3D.Application.DTOs.CustomRequests;

public class CustomRequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactWhatsApp { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Characteristics { get; set; }
    public string? DesiredColors { get; set; }
    public string? ApproximateSize { get; set; }
    public int Quantity { get; set; }
    public string? Observations { get; set; }
    public CustomRequestStatus Status { get; set; }
    public List<CustomRequestImageDto> Images { get; set; } = [];
    public List<CustomQuoteDto> Quotes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public record CustomRequestImageDto(Guid Id, string Url, string OriginalName);

public class CustomQuoteDto
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int DeadlineDays { get; set; }
    public string? Description { get; set; }
    public QuoteStatus Status { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ---- Criação ----

public record CreateCustomRequestRequest(
    string ProductType,
    string Description,
    string? Characteristics,
    string? DesiredColors,
    string? ApproximateSize,
    int Quantity,
    string? Observations,
    string ContactName,
    string ContactEmail,
    string? ContactWhatsApp
);

// ---- Admin ----

public record UpdateCustomRequestStatusRequest(
    CustomRequestStatus Status,
    string? AdminNotes
);

public record CreateQuoteRequest(
    decimal Price,
    int DeadlineDays,
    string? Description,
    string? AdminNotes
);

public record RespondQuoteRequest(bool Accepted);
