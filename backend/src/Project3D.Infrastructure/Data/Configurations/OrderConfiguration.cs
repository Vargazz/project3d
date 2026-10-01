using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project3D.Domain.Entities;

namespace Project3D.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(20).IsRequired();
        builder.Property(o => o.Subtotal).HasPrecision(18, 2).IsRequired();
        builder.Property(o => o.ShippingCost).HasPrecision(18, 2).IsRequired();
        builder.Property(o => o.Discount).HasPrecision(18, 2).IsRequired();
        builder.Property(o => o.Total).HasPrecision(18, 2).IsRequired();
        builder.Property(o => o.PaymentMethod).HasMaxLength(50);
        builder.Property(o => o.TrackingCode).HasMaxLength(100);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PaymentStatus);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
