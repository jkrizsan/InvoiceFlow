using InvoiceFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");
        builder.HasKey(line => line.Id);
        builder.Property(line => line.Description).HasMaxLength(300).IsRequired();
        builder.Property(line => line.Quantity).HasPrecision(18, 3);
        builder.Property(line => line.UnitPrice).HasPrecision(18, 2);
        builder.Property(line => line.VatCategory).HasConversion<int>();
        builder.Property(line => line.VatRate).HasPrecision(5, 2);
        builder.Property(line => line.NetAmount).HasPrecision(18, 2);
        builder.Property(line => line.VatAmount).HasPrecision(18, 2);
        builder.Property(line => line.GrossAmount).HasPrecision(18, 2);
    }
}

