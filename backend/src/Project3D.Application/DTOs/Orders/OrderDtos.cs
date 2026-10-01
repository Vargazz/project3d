using Project3D.Domain.Enums;

namespace Project3D.Application.DTOs.Orders;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TrackingCode { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
    public AddressDto? ShippingAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? VariantName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? CustomizationData,
    string? ProductImageUrl
);

public record AddressDto(
    Guid? Id,
    string ZipCode,
    string State,
    string City,
    string Neighborhood,
    string Street,
    string Number,
    string? Complement,
    string? Label
);

// ---- Criação de pedido (checkout) ----

public record CreateOrderRequest(
    List<CartItemRequest> Items,
    Guid? AddressId,
    AddressDto? NewAddress,
    Guid? ShippingOptionId,
    string? CouponCode,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string? CustomerCpf
);

public record CartItemRequest(
    Guid ProductId,
    Guid? VariantId,
    int Quantity,
    string? CustomizationData
);

// ---- Atualização de status (admin) ----

public record UpdateOrderStatusRequest(
    OrderStatus Status,
    string? Notes,
    string? TrackingCode
);
