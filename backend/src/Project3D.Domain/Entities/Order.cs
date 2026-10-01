using Project3D.Domain.Common;
using Project3D.Domain.Enums;

namespace Project3D.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;  // Ex: "PED-20240001"
    public Guid UserId { get; set; }
    public Guid? AddressId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Received;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; } = 0;
    public decimal Discount { get; set; } = 0;
    public decimal Total { get; set; }

    public string? PaymentMethod { get; set; }       // "pix", "credit_card", "debit_card"
    public string? TrackingCode { get; set; }
    public string? Notes { get; set; }
    public Guid? CouponId { get; set; }

    // Endereço snapshot (copiado no momento da compra)
    public string? ShippingAddressSnapshot { get; set; }  // JSON

    // Navegação (sem referência direta ao User — controlada pela Infrastructure)
    public virtual ICollection<OrderItem> Items { get; set; } = [];
    public virtual ICollection<Payment> Payments { get; set; } = [];
}
