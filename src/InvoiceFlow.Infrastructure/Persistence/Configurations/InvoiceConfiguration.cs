using InvoiceFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(invoice => invoice.Id);

        builder.Property(invoice => invoice.OwnerId).HasMaxLength(128).IsRequired();
        builder.Property(invoice => invoice.Number).HasMaxLength(40);
        builder.Property(invoice => invoice.Status).HasConversion<int>();
        builder.Property(invoice => invoice.Currency).HasMaxLength(3).IsRequired();

        builder.Property(invoice => invoice.SellerName).HasMaxLength(160).IsRequired();
        builder.Property(invoice => invoice.SellerBusinessId).HasMaxLength(40).IsRequired();
        builder.Property(invoice => invoice.SellerVatNumber).HasMaxLength(40).IsRequired();
        builder.Property(invoice => invoice.SellerAddress).HasMaxLength(300).IsRequired();
        builder.Property(invoice => invoice.SellerCountryCode).HasMaxLength(2).IsFixedLength().IsRequired();

        builder.Property(invoice => invoice.CustomerName).HasMaxLength(160).IsRequired();
        builder.Property(invoice => invoice.CustomerEmail).HasMaxLength(254).IsRequired();
        builder.Property(invoice => invoice.CustomerAddress).HasMaxLength(300).IsRequired();
        builder.Property(invoice => invoice.CustomerCountryCode).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(invoice => invoice.CustomerType).HasConversion<int>();
        builder.Property(invoice => invoice.CustomerVatNumber).HasMaxLength(40);
        builder.Property(invoice => invoice.VatTreatment).HasConversion<int>();

        builder.Property(invoice => invoice.NetTotal).HasPrecision(18, 2);
        builder.Property(invoice => invoice.VatTotal).HasPrecision(18, 2);
        builder.Property(invoice => invoice.GrossTotal).HasPrecision(18, 2);
        builder.Property(invoice => invoice.Notes).HasMaxLength(2000);
        builder.Property(invoice => invoice.Version).IsConcurrencyToken();

        builder.HasIndex(invoice => new { invoice.OwnerId, invoice.Status });
        builder.HasIndex(invoice => invoice.Number)
            .IsUnique()
            .HasFilter("\"Number\" IS NOT NULL");

        builder.HasMany(invoice => invoice.Lines)
            .WithOne()
            .HasForeignKey(line => line.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(invoice => invoice.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

