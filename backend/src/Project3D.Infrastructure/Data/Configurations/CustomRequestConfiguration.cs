using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project3D.Domain.Entities;

namespace Project3D.Infrastructure.Data.Configurations;

public class CustomRequestConfiguration : IEntityTypeConfiguration<CustomRequest>
{
    public void Configure(EntityTypeBuilder<CustomRequest> builder)
    {
        builder.ToTable("custom_requests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RequestNumber).HasMaxLength(20).IsRequired();
        builder.Property(r => r.ProductType).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).IsRequired();
        builder.Property(r => r.ContactName).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ContactEmail).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ContactWhatsApp).HasMaxLength(20);
        builder.Property(r => r.EstimatedBudget).HasPrecision(18, 2);

        builder.HasIndex(r => r.RequestNumber).IsUnique();
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Status);

        builder.HasMany(r => r.Images)
            .WithOne(i => i.CustomRequest)
            .HasForeignKey(i => i.CustomRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Quotes)
            .WithOne(q => q.CustomRequest)
            .HasForeignKey(q => q.CustomRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
