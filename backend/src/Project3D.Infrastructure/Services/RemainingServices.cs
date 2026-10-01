using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Project3D.Application.DTOs.CustomRequests;
using Project3D.Application.DTOs.Payments;
using Project3D.Application.DTOs.Products;
using Project3D.Application.DTOs.Users;
using Project3D.Application.Services.Interfaces;
using Project3D.Domain.Entities;
using Project3D.Domain.Enums;
using Project3D.Domain.Interfaces;
using Project3D.Infrastructure.Data;
using Project3D.Infrastructure.Identity;

namespace Project3D.Infrastructure.Services;

// ============================================================
// InventoryService
// ============================================================
public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    public InventoryService(AppDbContext db) => _db = db;

    public async Task<int> GetStockAsync(Guid productId, Guid? variantId = null, CancellationToken ct = default)
    {
        var inv = await _db.Inventories.FirstOrDefaultAsync(
            i => i.ProductId == productId && i.VariantId == variantId, ct);
        return inv?.Quantity ?? 0;
    }

    public async Task AdjustStockAsync(Guid productId, Guid? variantId, int quantity,
        InventoryMovementType type, string? reason = null, Guid? referenceId = null,
        Guid? adminId = null, CancellationToken ct = default)
    {
        var inv = await _db.Inventories.FirstOrDefaultAsync(
            i => i.ProductId == productId && i.VariantId == variantId, ct);

        if (inv is null)
        {
            inv = new Inventory { ProductId = productId, VariantId = variantId };
            await _db.Inventories.AddAsync(inv, ct);
            await _db.SaveChangesAsync(ct);
        }

        var delta = type is InventoryMovementType.Sale ? -quantity : quantity;
        inv.Quantity = Math.Max(0, inv.Quantity + delta);

        await _db.InventoryMovements.AddAsync(new InventoryMovement
        {
            InventoryId = inv.Id,
            MovementType = type,
            Quantity = delta,
            Reason = reason,
            ReferenceId = referenceId,
            CreatedById = adminId
        }, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(CancellationToken ct = default)
        => await _db.Inventories
            .Include(i => i.Product)
            .Where(i => i.Quantity <= i.MinStock && i.VariantId == null)
            .Select(i => new LowStockAlertDto(i.ProductId, i.Product.Name, i.Product.Sku, i.Quantity, i.MinStock))
            .ToListAsync(ct);
}

// ============================================================
// CouponService
// ============================================================
public class CouponService : ICouponService
{
    private readonly AppDbContext _db;
    public CouponService(AppDbContext db) => _db = db;

    public async Task<CouponValidationResult> ValidateAsync(string code, decimal orderTotal, CancellationToken ct = default)
    {
        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant() && c.IsActive, ct);
        if (coupon is null) return new(false, "Cupom inválido.", 0, null);

        var now = DateTime.UtcNow;
        if (coupon.ValidFrom.HasValue && now < coupon.ValidFrom) return new(false, "Cupom ainda não é válido.", 0, null);
        if (coupon.ValidUntil.HasValue && now > coupon.ValidUntil) return new(false, "Cupom expirado.", 0, null);
        if (coupon.MaxUses.HasValue && coupon.UsedCount >= coupon.MaxUses) return new(false, "Cupom esgotado.", 0, null);
        if (coupon.MinOrderValue.HasValue && orderTotal < coupon.MinOrderValue)
            return new(false, $"Pedido mínimo de R$ {coupon.MinOrderValue:F2}.", 0, null);

        var discount = coupon.Type == CouponType.Percent ? orderTotal * (coupon.Value / 100) : coupon.Value;
        return new(true, null, Math.Min(discount, orderTotal), coupon.Id);
    }

    public async Task<Coupon> CreateAsync(CreateCouponRequest req, CancellationToken ct = default)
    {
        if (await _db.Coupons.AnyAsync(c => c.Code == req.Code.ToUpperInvariant(), ct))
            throw new InvalidOperationException($"Cupom '{req.Code}' já existe.");
        var coupon = new Coupon { Code = req.Code.ToUpperInvariant(), Type = req.Type, Value = req.Value, MinOrderValue = req.MinOrderValue, MaxUses = req.MaxUses, ValidFrom = req.ValidFrom, ValidUntil = req.ValidUntil, IsActive = req.IsActive };
        await _db.Coupons.AddAsync(coupon, ct);
        await _db.SaveChangesAsync(ct);
        return coupon;
    }

    public async Task<Coupon> UpdateAsync(Guid id, CreateCouponRequest req, CancellationToken ct = default)
    {
        var coupon = await _db.Coupons.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException("Cupom não encontrado.");
        coupon.Code = req.Code.ToUpperInvariant(); coupon.Type = req.Type; coupon.Value = req.Value;
        coupon.MinOrderValue = req.MinOrderValue; coupon.MaxUses = req.MaxUses;
        coupon.ValidFrom = req.ValidFrom; coupon.ValidUntil = req.ValidUntil; coupon.IsActive = req.IsActive;
        await _db.SaveChangesAsync(ct);
        return coupon;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var coupon = await _db.Coupons.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException("Cupom não encontrado.");
        _db.Coupons.Remove(coupon);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync(CancellationToken ct = default)
        => await _db.Coupons.OrderByDescending(c => c.CreatedAt).ToListAsync(ct);
}

// ============================================================
// UserService
// ============================================================
public class UserService : IUserService
{
    private readonly AppDbContext _db;
    public UserService(AppDbContext db) => _db = db;

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
        return u is null ? null : Map(u);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid id, UpdateProfileRequest req, CancellationToken ct = default)
    {
        var u = await _db.Users.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException("Usuário não encontrado.");
        u.FullName = req.FullName;
        u.PhoneNumber = req.Phone;
        u.Cpf = req.Cpf;
        await _db.SaveChangesAsync(ct);
        return Map(u);
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var q = _db.Users.AsNoTracking().Where(u => u.Role == UserRole.Customer).OrderBy(u => u.FullName);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<UserDto>(items.Select(Map), total, page, pageSize);
    }

    public async Task ToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        var u = await _db.Users.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException("Usuário não encontrado.");
        u.IsActive = !u.IsActive;
        await _db.SaveChangesAsync(ct);
    }

    private static UserDto Map(ApplicationUser u) => new()
    {
        Id = u.Id, FullName = u.FullName, Email = u.Email ?? string.Empty,
        Phone = u.PhoneNumber, Cpf = u.Cpf, Role = u.Role, IsActive = u.IsActive, CreatedAt = u.CreatedAt
    };
}

// ============================================================
// CategoryService
// ============================================================
public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    public CategoryService(AppDbContext db) => _db = db;

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = _db.Categories.Include(c => c.Subcategories).AsNoTracking();
        if (activeOnly) q = q.Where(c => c.IsActive);
        return (await q.OrderBy(c => c.DisplayOrder).ToListAsync(ct)).Select(Map);
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var c = await _db.Categories.Include(c => c.Subcategories).AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug, ct);
        return c is null ? null : Map(c);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest req, CancellationToken ct = default)
    {
        var slug = Slugify(req.Name);
        var cat = new Category { Name = req.Name, Slug = slug, Description = req.Description, IsActive = req.IsActive, DisplayOrder = req.DisplayOrder };
        await _db.Categories.AddAsync(cat, ct);
        await _db.SaveChangesAsync(ct);
        return Map(cat);
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, CreateCategoryRequest req, CancellationToken ct = default)
    {
        var cat = await _db.Categories.Include(c => c.Subcategories).FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new KeyNotFoundException("Categoria não encontrada.");
        cat.Name = req.Name; cat.Description = req.Description; cat.IsActive = req.IsActive; cat.DisplayOrder = req.DisplayOrder;
        await _db.SaveChangesAsync(ct);
        return Map(cat);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var cat = await _db.Categories.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException("Categoria não encontrada.");
        if (await _db.Products.AnyAsync(p => p.CategoryId == id, ct))
            throw new InvalidOperationException("Não é possível excluir uma categoria com produtos.");
        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync(ct);
    }

    private static CategoryDto Map(Category c) => new(
        c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.IsActive, c.DisplayOrder,
        c.Subcategories.Select(s => new SubcategoryDto(s.Id, s.CategoryId, s.Name, s.Slug, s.IsActive)).ToList());

    private static string Slugify(string name)
    {
        var s = name.ToLowerInvariant();
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[àáâãä]", "a");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[èéêë]", "e");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[ìíîï]", "i");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[òóôõö]", "o");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[ùúûü]", "u");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[ç]", "c");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9\s-]", "");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"\s+", "-");
        return s.Trim('-');
    }
}

// ============================================================
// DashboardService
// ============================================================
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalRevenue = await _db.Orders.Where(o => o.PaymentStatus == PaymentStatus.Paid).SumAsync(o => (decimal?)o.Total, ct) ?? 0;
        var revenueThisMonth = await _db.Orders.Where(o => o.PaymentStatus == PaymentStatus.Paid && o.CreatedAt >= monthStart).SumAsync(o => (decimal?)o.Total, ct) ?? 0;
        var pendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Received || o.Status == OrderStatus.PaymentPending, ct);
        var inProductionOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.InProduction, ct);
        var shippedOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Shipped, ct);
        var pendingCustom = await _db.CustomRequests.CountAsync(r => r.Status == CustomRequestStatus.Received || r.Status == CustomRequestStatus.UnderAnalysis, ct);
        var lowStock = await _db.Inventories.CountAsync(i => i.Quantity > 0 && i.Quantity <= i.MinStock && i.VariantId == null, ct);
        var outOfStock = await _db.Inventories.CountAsync(i => i.Quantity == 0 && i.VariantId == null, ct);

        var topProducts = await _db.Products.Where(p => p.IsActive).OrderByDescending(p => p.SoldCount).Take(5)
            .Select(p => new TopProductDto(p.Id, p.Name, p.SoldCount, 0m)).ToListAsync(ct);

        var last30 = now.AddDays(-30);
        // EF Core não consegue traduzir GroupBy com projeção complexa para SQL;
        // trazemos os campos necessários para memória e agrupamos no cliente.
        var rawOrders = await _db.Orders
            .Where(o => o.PaymentStatus == PaymentStatus.Paid && o.CreatedAt >= last30)
            .Select(o => new { o.CreatedAt, o.Total })
            .ToListAsync(ct);
        var salesByDay = rawOrders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesByDayDto(g.Key, g.Sum(o => o.Total), g.Count()))
            .OrderBy(s => s.Date).ToList();

        // OrdersByStatus — trazer para memória também (GroupBy com ToString não é traduzível)
        var rawStatus = await _db.Orders
            .Select(o => o.Status)
            .ToListAsync(ct);
        var ordersByStatus = rawStatus
            .GroupBy(s => s)
            .Select(g => new OrdersByStatusDto(g.Key.ToString(), g.Count()))
            .ToList();

        return new DashboardSummaryDto(totalRevenue, revenueThisMonth, pendingOrders, inProductionOrders,
            shippedOrders, pendingCustom, lowStock, outOfStock, topProducts, salesByDay, ordersByStatus);
    }
}

// ============================================================
// StoreSettingService
// ============================================================
public class StoreSettingService : IStoreSettingService
{
    private readonly AppDbContext _db;
    public StoreSettingService(AppDbContext db) => _db = db;

    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
        => (await _db.StoreSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == key, ct))?.Value;

    public async Task SetAsync(string key, string value, CancellationToken ct = default)
    {
        var s = await _db.StoreSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (s is null) { s = new StoreSetting { Key = key }; await _db.StoreSettings.AddAsync(s, ct); }
        s.Value = value;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Dictionary<string, string?>> GetAllAsync(CancellationToken ct = default)
        => await _db.StoreSettings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value, ct);

    public async Task SetBulkAsync(Dictionary<string, string> settings, CancellationToken ct = default)
    { foreach (var kv in settings) await SetAsync(kv.Key, kv.Value, ct); }
}

// ============================================================
// CustomRequestService
// ============================================================
public class CustomRequestService : ICustomRequestService
{
    private readonly AppDbContext _db;
    private readonly IStorageService _storage;
    private readonly INotificationService _notifications;

    public CustomRequestService(AppDbContext db, IStorageService storage, INotificationService notifications)
    { _db = db; _storage = storage; _notifications = notifications; }

    public async Task<CustomRequestDto> CreateAsync(Guid userId, CreateCustomRequestRequest req, CancellationToken ct = default)
    {
        var number = $"PERS-{DateTime.UtcNow.Year}{(await _db.CustomRequests.CountAsync(ct) + 1):D4}";
        var entity = new CustomRequest
        {
            RequestNumber = number, UserId = userId, ProductType = req.ProductType,
            Description = req.Description, Characteristics = req.Characteristics,
            DesiredColors = req.DesiredColors, ApproximateSize = req.ApproximateSize,
            Quantity = req.Quantity,
            Observations = req.Observations,
            ContactName = req.ContactName, ContactEmail = req.ContactEmail,
            ContactWhatsApp = req.ContactWhatsApp, Status = CustomRequestStatus.Received
        };
        await _db.CustomRequests.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return (await GetByIdAsync(entity.Id, ct))!;
    }

    public async Task<CustomRequestDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.CustomRequests.Include(r => r.Images).Include(r => r.Quotes)
            .AsNoTracking().FirstOrDefaultAsync(r => r.Id == id, ct);
        return e is null ? null : Map(e);
    }

    public async Task<PagedResult<CustomRequestDto>> GetUserRequestsAsync(Guid userId, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var q = _db.CustomRequests.Include(r => r.Images).Include(r => r.Quotes)
            .AsNoTracking().Where(r => r.UserId == userId).OrderByDescending(r => r.CreatedAt);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<CustomRequestDto>(items.Select(Map), total, page, pageSize);
    }

    public async Task<PagedResult<CustomRequestDto>> GetAllRequestsAsync(int page = 1, int pageSize = 20, string? status = null, CancellationToken ct = default)
    {
        var q = _db.CustomRequests.Include(r => r.Images).Include(r => r.Quotes)
            .AsNoTracking().OrderByDescending(r => r.CreatedAt);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CustomRequestStatus>(status, true, out var st))
            q = (IOrderedQueryable<CustomRequest>)q.Where(r => r.Status == st);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<CustomRequestDto>(items.Select(Map), total, page, pageSize);
    }

    public async Task<CustomRequestDto> UpdateStatusAsync(Guid id, UpdateCustomRequestStatusRequest req, Guid adminId, CancellationToken ct = default)
    {
        var entity = await _db.CustomRequests.FindAsync(new object[] { id }, ct) ?? throw new KeyNotFoundException("Solicitação não encontrada.");
        entity.Status = req.Status;
        await _db.AdminLogs.AddAsync(new AdminLog { AdminId = adminId, Action = "UPDATE_CUSTOM_REQUEST_STATUS", Entity = "CustomRequest", EntityId = id }, ct);
        await _db.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<string> AddImageAsync(Guid requestId, Stream stream, string fileName, CancellationToken ct = default)
    {
        _ = await _db.CustomRequests.FindAsync(new object[] { requestId }, ct) ?? throw new KeyNotFoundException("Solicitação não encontrada.");
        var result = await _storage.UploadAsync(stream, fileName, "custom-requests", ct);
        await _db.CustomRequestImages.AddAsync(new CustomRequestImage
        { CustomRequestId = requestId, Url = result.Url, PublicId = result.PublicId, OriginalName = fileName }, ct);
        await _db.SaveChangesAsync(ct);
        return result.Url;
    }

    public async Task<CustomQuoteDto> CreateQuoteAsync(Guid requestId, CreateQuoteRequest req, Guid adminId, CancellationToken ct = default)
    {
        var entity = await _db.CustomRequests.FindAsync(new object[] { requestId }, ct) ?? throw new KeyNotFoundException("Solicitação não encontrada.");
        var quote = new CustomQuote
        {
            CustomRequestId = requestId, Price = req.Price, DeadlineDays = req.DeadlineDays,
            Description = req.Description, AdminNotes = req.AdminNotes,
            Status = QuoteStatus.Sent, SentAt = DateTime.UtcNow, CreatedById = adminId
        };
        await _db.CustomQuotes.AddAsync(quote, ct);
        entity.Status = CustomRequestStatus.QuoteSent;
        await _notifications.SendAsync(new SendNotificationRequest(
            entity.UserId, "Orçamento disponível!",
            $"Orçamento para '{entity.ProductType}' pronto. Valor: R$ {req.Price:F2}",
            NotificationType.QuoteAvailable, "CustomRequest", requestId), ct);
        await _db.SaveChangesAsync(ct);
        return MapQuote(quote);
    }

    public async Task<CustomQuoteDto> RespondToQuoteAsync(Guid quoteId, Guid userId, RespondQuoteRequest req, CancellationToken ct = default)
    {
        var quote = await _db.CustomQuotes.Include(q => q.CustomRequest)
            .FirstOrDefaultAsync(q => q.Id == quoteId, ct) ?? throw new KeyNotFoundException("Orçamento não encontrado.");
        if (quote.CustomRequest.UserId != userId) throw new UnauthorizedAccessException("Sem permissão.");
        quote.Status = req.Accepted ? QuoteStatus.Accepted : QuoteStatus.Rejected;
        quote.RespondedAt = DateTime.UtcNow;
        quote.CustomRequest.Status = req.Accepted ? CustomRequestStatus.Approved : CustomRequestStatus.Received;
        await _db.SaveChangesAsync(ct);
        return MapQuote(quote);
    }

    private static CustomRequestDto Map(CustomRequest r) => new()
    {
        Id = r.Id, RequestNumber = r.RequestNumber, UserId = r.UserId,
        CustomerName = r.ContactName, ContactEmail = r.ContactEmail, ContactWhatsApp = r.ContactWhatsApp,
        ProductType = r.ProductType, Description = r.Description, Characteristics = r.Characteristics,
        DesiredColors = r.DesiredColors, ApproximateSize = r.ApproximateSize, Quantity = r.Quantity,
        Observations = r.Observations,
        Status = r.Status,
        Images = r.Images.Select(i => new CustomRequestImageDto(i.Id, i.Url, i.OriginalName)).ToList(),
        Quotes = r.Quotes.OrderByDescending(q => q.CreatedAt).Select(MapQuote).ToList(),
        CreatedAt = r.CreatedAt
    };

    private static CustomQuoteDto MapQuote(CustomQuote q) => new()
    { Id = q.Id, Price = q.Price, DeadlineDays = q.DeadlineDays, Description = q.Description,
      Status = q.Status, SentAt = q.SentAt, RespondedAt = q.RespondedAt, CreatedAt = q.CreatedAt };
}

// ============================================================
// PaymentProcessingService
// ============================================================
public class PaymentProcessingService : IPaymentProcessingService
{
    private readonly AppDbContext _db;
    private readonly IPaymentService _mp;
    private readonly IInventoryService _inventory;
    private readonly INotificationService _notifications;
    private readonly IConfiguration _config;

    public PaymentProcessingService(AppDbContext db, IPaymentService mp, IInventoryService inventory,
        INotificationService notifications, IConfiguration config)
    { _db = db; _mp = mp; _inventory = inventory; _notifications = notifications; _config = config; }

    public async Task<PaymentPreferenceResponse> CreateOrderPreferenceAsync(Guid orderId, Guid userId, CancellationToken ct = default)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct)
            ?? throw new KeyNotFoundException("Pedido não encontrado.");
        if (order.PaymentStatus == PaymentStatus.Paid) throw new InvalidOperationException("Pedido já pago.");
        var appUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        var result = await _mp.CreatePreferenceAsync(new CreatePreferenceRequest(
            order.OrderNumber, $"Pedido {order.OrderNumber} - Print3D Studio",
            order.Total, appUser.Email!, appUser.FullName), ct);

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending, ct);
        if (payment is null) { payment = new Payment { OrderId = orderId, Amount = order.Total, Status = PaymentStatus.Pending }; await _db.Payments.AddAsync(payment, ct); }
        payment.PreferenceId = result.PreferenceId;
        order.PaymentStatus = PaymentStatus.WaitingPayment;
        await _db.SaveChangesAsync(ct);

        var useSandbox = string.IsNullOrWhiteSpace(_config["MercadoPago:AccessToken"]);
        return new PaymentPreferenceResponse(result.PreferenceId, useSandbox ? result.SandboxInitPoint : result.InitPoint, result.SandboxInitPoint);
    }

    public async Task<PaymentPreferenceResponse> CreateQuotePreferenceAsync(Guid quoteId, Guid userId, CancellationToken ct = default)
    {
        var quote = await _db.CustomQuotes.Include(q => q.CustomRequest)
            .FirstOrDefaultAsync(q => q.Id == quoteId && q.CustomRequest.UserId == userId, ct)
            ?? throw new KeyNotFoundException("Orçamento não encontrado.");
        if (quote.Status != QuoteStatus.Accepted) throw new InvalidOperationException("Orçamento precisa ser aceito primeiro.");
        var appUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        var result = await _mp.CreatePreferenceAsync(new CreatePreferenceRequest(
            $"PERS-{quoteId}", "Orçamento personalizado - Print3D Studio",
            quote.Price, appUser.Email!, appUser.FullName), ct);

        var payment = new Payment { CustomQuoteId = quoteId, Amount = quote.Price, Status = PaymentStatus.Pending, PreferenceId = result.PreferenceId };
        await _db.Payments.AddAsync(payment, ct);
        quote.CustomRequest.Status = CustomRequestStatus.WaitingPayment;
        await _db.SaveChangesAsync(ct);

        var useSandbox = string.IsNullOrWhiteSpace(_config["MercadoPago:AccessToken"]);
        return new PaymentPreferenceResponse(result.PreferenceId, useSandbox ? result.SandboxInitPoint : result.InitPoint, result.SandboxInitPoint);
    }

    public async Task ProcessWebhookAsync(string paymentId, CancellationToken ct = default)
    {
        var info = await _mp.GetPaymentInfoAsync(paymentId, ct);
        if (info is null) return;

        var payment = await _db.Payments
            .Include(p => p.Order).ThenInclude(o => o!.Items)
            .Include(p => p.CustomQuote).ThenInclude(q => q!.CustomRequest)
            .FirstOrDefaultAsync(p => p.ExternalId == paymentId, ct);

        if (payment is null) return;

        payment.ExternalId = info.Id;
        payment.Method = info.PaymentMethod;
        payment.RawResponse = System.Text.Json.JsonSerializer.Serialize(info);

        switch (info.Status)
        {
            case "approved":
                payment.Status = PaymentStatus.Paid;
                payment.PaidAt = info.DateApproved ?? DateTime.UtcNow;
                if (payment.Order is not null)
                {
                    payment.Order.PaymentStatus = PaymentStatus.Paid;
                    payment.Order.Status = OrderStatus.PaymentApproved;
                    payment.Order.PaymentMethod = info.PaymentMethod;
                    foreach (var item in payment.Order.Items)
                        await _inventory.AdjustStockAsync(item.ProductId, item.VariantId, item.Quantity,
                            InventoryMovementType.Sale, $"Venda {payment.Order.OrderNumber}", payment.Order.Id, null, ct);
                    await _notifications.SendAsync(new SendNotificationRequest(
                        payment.Order.UserId, "Pagamento aprovado!",
                        "Seu pagamento foi confirmado. Pedido em produção.",
                        NotificationType.PaymentUpdate, "Order", payment.OrderId), ct);
                }
                if (payment.CustomQuote is not null)
                {
                    payment.CustomQuote.CustomRequest.Status = CustomRequestStatus.InProduction;
                    await _notifications.SendAsync(new SendNotificationRequest(
                        payment.CustomQuote.CustomRequest.UserId, "Pagamento aprovado!",
                        "Produto personalizado em produção.",
                        NotificationType.PaymentUpdate, "CustomRequest", payment.CustomQuote.CustomRequestId), ct);
                }
                break;
            case "rejected": payment.Status = PaymentStatus.Rejected; if (payment.Order is not null) payment.Order.PaymentStatus = PaymentStatus.Rejected; break;
            case "cancelled": payment.Status = PaymentStatus.Cancelled; if (payment.Order is not null) payment.Order.PaymentStatus = PaymentStatus.Cancelled; break;
            case "refunded": payment.Status = PaymentStatus.Refunded; if (payment.Order is not null) payment.Order.PaymentStatus = PaymentStatus.Refunded; break;
        }
        await _db.SaveChangesAsync(ct);
    }
}

// ============================================================
// AdminUserService
// ============================================================
public class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var admins = await _db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Admin)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);
        return admins.Select(u => new AdminUserDto(u.Id, u.FullName, u.Email ?? string.Empty, u.IsActive, u.CreatedAt));
    }

    public async Task<AdminUserDto> CreateAsync(CreateAdminRequest req, CancellationToken ct = default)
    {
        var existing = await _userManager.FindByEmailAsync(req.Email);
        if (existing is not null)
            throw new InvalidOperationException("Já existe um usuário com este e-mail.");

        var user = new ApplicationUser
        {
            FullName = req.FullName,
            Email = req.Email,
            UserName = req.Email,
            EmailConfirmed = true,
            Role = UserRole.Admin,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Admin");
        return new AdminUserDto(user.Id, user.FullName, user.Email ?? string.Empty, user.IsActive, user.CreatedAt);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException("Administrador não encontrado.");
        await _userManager.DeleteAsync(user);
    }

    public async Task ResetPasswordAsync(Guid id, string newPassword, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException("Administrador não encontrado.");
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
