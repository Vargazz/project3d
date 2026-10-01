using Microsoft.EntityFrameworkCore;
using Project3D.Application.DTOs.Orders;
using Project3D.Application.DTOs.Products;
using Project3D.Application.Services.Interfaces;
using Project3D.Domain.Entities;
using Project3D.Domain.Enums;
using Project3D.Domain.Interfaces;
using Project3D.Infrastructure.Data;

namespace Project3D.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IInventoryService _inventory;
    private readonly ICouponService _coupon;
    private readonly INotificationService _notifications;

    public OrderService(AppDbContext db, IInventoryService inventory, ICouponService coupon, INotificationService notifications)
    { _db = db; _inventory = inventory; _coupon = coupon; _notifications = notifications; }

    public async Task<OrderDto> CreateAsync(Guid userId, CreateOrderRequest request, CancellationToken ct = default)
    {
        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var product = await _db.Products.Include(p => p.Images).Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.IsActive, ct)
                ?? throw new KeyNotFoundException($"Produto {item.ProductId} não encontrado.");

            var stock = await _inventory.GetStockAsync(item.ProductId, item.VariantId, ct);
            if (stock < item.Quantity) throw new InvalidOperationException($"Estoque insuficiente para '{product.Name}'.");

            var unitPrice = product.PromotionalPrice ?? product.Price;
            if (item.VariantId.HasValue)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == item.VariantId);
                if (variant is not null) unitPrice += variant.PriceModifier;
            }

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId, VariantId = item.VariantId, Quantity = item.Quantity,
                UnitPrice = unitPrice, TotalPrice = unitPrice * item.Quantity,
                CustomizationData = item.CustomizationData, ProductNameSnapshot = product.Name,
                VariantNameSnapshot = item.VariantId.HasValue ? product.Variants.FirstOrDefault(v => v.Id == item.VariantId)?.Value : null
            });
            subtotal += unitPrice * item.Quantity;
        }

        decimal shippingCost = 0;
        if (request.ShippingOptionId.HasValue)
        {
            var shipping = await _db.ShippingOptions.FindAsync(new object[] { request.ShippingOptionId }, ct);
            shippingCost = shipping?.Price ?? 0;
        }

        decimal discount = 0;
        Guid? couponId = null;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var couponResult = await _coupon.ValidateAsync(request.CouponCode, subtotal + shippingCost, ct);
            if (!couponResult.IsValid) throw new InvalidOperationException(couponResult.ErrorMessage);
            discount = couponResult.DiscountAmount;
            couponId = couponResult.CouponId;
        }

        Guid? addressId = request.AddressId;
        if (!addressId.HasValue && request.NewAddress is not null)
        {
            var addr = new Address
            {
                UserId = userId, Label = "Entrega",
                ZipCode = request.NewAddress.ZipCode, State = request.NewAddress.State,
                City = request.NewAddress.City, Neighborhood = request.NewAddress.Neighborhood,
                Street = request.NewAddress.Street, Number = request.NewAddress.Number,
                Complement = request.NewAddress.Complement
            };
            await _db.Addresses.AddAsync(addr, ct);
            await _db.SaveChangesAsync(ct);
            addressId = addr.Id;
        }

        var total = subtotal + shippingCost - discount;
        var orderNumber = $"PED-{DateTime.UtcNow.Year}{(await _db.Orders.CountAsync(ct) + 1):D5}";

        var order = new Order
        {
            OrderNumber = orderNumber, UserId = userId, AddressId = addressId,
            Status = OrderStatus.Received, PaymentStatus = PaymentStatus.Pending,
            Subtotal = subtotal, ShippingCost = shippingCost, Discount = discount,
            Total = total, CouponId = couponId, Items = orderItems
        };

        if (addressId.HasValue)
        {
            var addr = await _db.Addresses.FindAsync(new object[] { addressId }, ct);
            if (addr is not null)
                order.ShippingAddressSnapshot = System.Text.Json.JsonSerializer.Serialize(new
                { addr.Street, addr.Number, addr.Complement, addr.Neighborhood, addr.City, addr.State, addr.ZipCode });
        }

        await _db.Orders.AddAsync(order, ct);
        if (couponId.HasValue) { var c = await _db.Coupons.FindAsync(new object[] { couponId }, ct); if (c is not null) c.UsedCount++; }
        await _db.SaveChangesAsync(ct);

        await _notifications.SendAsync(new SendNotificationRequest(
            userId, "Pedido realizado!", $"Pedido {orderNumber} recebido. Aguardando pagamento.",
            NotificationType.OrderUpdate, "Order", order.Id), ct);

        return (await GetByIdAsync(order.Id, ct))!;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var o = await GetQ().FirstOrDefaultAsync(x => x.Id == id, ct);
        return o is null ? null : Map(o);
    }

    public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var o = await GetQ().FirstOrDefaultAsync(x => x.OrderNumber == orderNumber, ct);
        return o is null ? null : Map(o);
    }

    public async Task<PagedResult<OrderDto>> GetUserOrdersAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var q = GetQ().Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<OrderDto>(items.Select(Map), total, page, pageSize);
    }

    public async Task<PagedResult<OrderDto>> GetAllOrdersAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken ct = default)
    {
        IQueryable<Order> q = GetQ().OrderByDescending(o => o.CreatedAt);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var st))
            q = q.Where(o => o.Status == st);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<OrderDto>(items.Select(Map), total, page, pageSize);
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, Guid adminId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException("Pedido não encontrado.");
        var oldStatus = order.Status;
        order.Status = request.Status;
        if (!string.IsNullOrWhiteSpace(request.Notes)) order.Notes = request.Notes;
        if (!string.IsNullOrWhiteSpace(request.TrackingCode)) order.TrackingCode = request.TrackingCode;
        await _db.AdminLogs.AddAsync(new AdminLog { AdminId = adminId, Action = "UPDATE_ORDER_STATUS", Entity = "Order", EntityId = id,
            Details = System.Text.Json.JsonSerializer.Serialize(new { OldStatus = oldStatus.ToString(), NewStatus = request.Status.ToString() }) }, ct);
        await _db.SaveChangesAsync(ct);

        var msg = request.Status switch
        {
            OrderStatus.PaymentApproved => "Seu pagamento foi aprovado!",
            OrderStatus.InProduction    => "Seu pedido está em produção.",
            OrderStatus.Shipped        => $"Pedido enviado! Código: {request.TrackingCode}",
            OrderStatus.Delivered      => "Pedido entregue.",
            OrderStatus.Cancelled      => "Pedido cancelado.",
            _ => $"Status atualizado: {request.Status}"
        };
        await _notifications.SendAsync(new SendNotificationRequest(order.UserId, "Atualização do pedido", msg,
            NotificationType.OrderUpdate, "Order", id), ct);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task CancelAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");
        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new InvalidOperationException("Não é possível cancelar pedido enviado ou entregue.");
        order.Status = OrderStatus.Cancelled;
        order.PaymentStatus = PaymentStatus.Cancelled;
        foreach (var item in order.Items)
            await _inventory.AdjustStockAsync(item.ProductId, item.VariantId, item.Quantity,
                InventoryMovementType.Cancellation, $"Cancelamento {order.OrderNumber}", order.Id, null, ct);
        await _db.SaveChangesAsync(ct);
    }

    private IQueryable<Order> GetQ()
        => _db.Orders.Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(o => o.Payments).AsNoTracking();

    private OrderDto Map(Order o)
    {
        var address = o.AddressId.HasValue ? _db.Addresses.AsNoTracking().FirstOrDefault(a => a.Id == o.AddressId) : null;
        var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == o.UserId);
        return new OrderDto
        {
            Id = o.Id, OrderNumber = o.OrderNumber, UserId = o.UserId,
            CustomerName = user?.FullName ?? string.Empty, CustomerEmail = user?.Email,
            Status = o.Status, PaymentStatus = o.PaymentStatus,
            Subtotal = o.Subtotal, ShippingCost = o.ShippingCost, Discount = o.Discount, Total = o.Total,
            PaymentMethod = o.PaymentMethod, TrackingCode = o.TrackingCode,
            CreatedAt = o.CreatedAt, UpdatedAt = o.UpdatedAt,
            ShippingAddress = address is null ? null : new AddressDto(address.Id, address.ZipCode, address.State,
                address.City, address.Neighborhood, address.Street, address.Number, address.Complement, address.Label),
            Items = o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductNameSnapshot, i.VariantNameSnapshot,
                i.Quantity, i.UnitPrice, i.TotalPrice, i.CustomizationData,
                i.Product?.Images.OrderBy(img => img.DisplayOrder).Select(img => img.Url).FirstOrDefault())).ToList()
        };
    }
}
