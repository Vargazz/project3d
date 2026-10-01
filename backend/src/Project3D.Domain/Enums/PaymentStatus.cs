namespace Project3D.Domain.Enums;

public enum PaymentStatus
{
    Pending = 1,          // Pendente
    WaitingPayment = 2,   // Aguardando pagamento
    Paid = 3,             // Pago
    Rejected = 4,         // Recusado
    Cancelled = 5,        // Cancelado
    Refunded = 6          // Estornado
}
