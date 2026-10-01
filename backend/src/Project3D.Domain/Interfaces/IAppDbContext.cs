using Microsoft.EntityFrameworkCore;
using Project3D.Domain.Entities;

namespace Project3D.Domain.Interfaces;

/// <summary>
/// Abstração do DbContext usada nos services da Infrastructure.
/// Permite injeção sem acoplamento ao tipo concreto AppDbContext.
/// </summary>
public interface IAppDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Subcategory> Subcategories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductCustomizationField> ProductCustomizationFields { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<InventoryMovement> InventoryMovements { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<CustomRequest> CustomRequests { get; }
    DbSet<CustomRequestImage> CustomRequestImages { get; }
    DbSet<CustomQuote> CustomQuotes { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<ShippingOption> ShippingOptions { get; }
    DbSet<StoreSetting> StoreSettings { get; }
    DbSet<AdminLog> AdminLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
