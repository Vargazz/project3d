using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project3D.Domain.Entities;

namespace Project3D.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Provider).HasMaxLength(50).IsRequired();
        builder.Property(p => p.ExternalId).HasMaxLength(100);
        builder.Property(p => p.PreferenceId).HasMaxLength(100);
        builder.Property(p => p.Method).HasMaxLength(50);
        builder.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();

        builder.HasIndex(p => p.ExternalId);
        builder.HasIndex(p => p.PreferenceId);
        builder.HasIndex(p => p.Status);

        // Relação com Order é opcional (pode ser pagamento de orçamento personalizado)
        builder.HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(p => p.CustomQuote)
            .WithMany(q => q.Payments)
            .HasForeignKey(p => p.CustomQuoteId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
