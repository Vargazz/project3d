namespace Project3D.Domain.Enums;

public enum QuoteStatus
{
    Draft = 1,        // Rascunho (admin trabalhando)
    Sent = 2,         // Enviado ao cliente
    Accepted = 3,     // Aceito pelo cliente
    Rejected = 4,     // Recusado pelo cliente
    Cancelled = 5     // Cancelado pelo admin
}
