namespace Project3D.Domain.Enums;

public enum OrderStatus
{
    Received = 1,        // Pedido recebido
    PaymentPending = 2,  // Aguardando pagamento
    PaymentApproved = 3, // Pagamento aprovado
    InProduction = 4,    // Em produção
    Ready = 5,           // Pronto
    Shipped = 6,         // Enviado
    Delivered = 7,       // Entregue
    Cancelled = 8        // Cancelado
}
