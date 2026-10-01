namespace Project3D.Domain.Enums;

public enum CustomRequestStatus
{
    Received = 1,            // Recebido
    UnderAnalysis = 2,       // Em análise
    WaitingInfo = 3,         // Aguardando informações
    QuoteSent = 4,           // Orçamento enviado
    WaitingApproval = 5,     // Aguardando aprovação
    Approved = 6,            // Aprovado
    WaitingPayment = 7,      // Aguardando pagamento
    Paid = 8,                // Pago
    InProduction = 9,        // Em produção
    Finished = 10,           // Finalizado
    Shipped = 11,            // Enviado
    Cancelled = 12           // Cancelado
}
