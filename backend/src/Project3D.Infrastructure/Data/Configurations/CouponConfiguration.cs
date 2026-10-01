using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project3D.Domain.Entities;

namespace Project3D.Infrastructure.Data.Configurations;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("coupons");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Value).HasPrecision(10, 2).IsRequired();
        builder.Property(c => c.MinOrderValue).HasPrecision(18, 2);

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.IsActive);
    }
}
