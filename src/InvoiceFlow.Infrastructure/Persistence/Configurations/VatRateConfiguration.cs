using InvoiceFlow.Domain.Entities;
using InvoiceFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceFlow.Infrastructure.Persistence.Configurations;

public sealed class VatRateConfiguration : IEntityTypeConfiguration<VatRate>
{
    private static readonly DateOnly EffectiveFrom = new(2026, 1, 1);

    public void Configure(EntityTypeBuilder<VatRate> builder)
    {
        builder.ToTable("VatRates");
        builder.HasKey(rate => rate.Id);
        builder.Property(rate => rate.CountryCode).HasMaxLength(2).IsFixedLength().IsRequired();
        builder.Property(rate => rate.Category).HasConversion<int>();
        builder.Property(rate => rate.Rate).HasPrecision(5, 2);
        builder.HasIndex(rate => new { rate.CountryCode, rate.Category, rate.ValidFrom }).IsUnique();

        builder.HasData(StandardRates());
    }

    private static VatRate[] StandardRates()
    {
        (string Country, decimal Rate)[] rates =
        [
            ("AT", 20m), ("BE", 21m), ("BG", 20m), ("HR", 25m),
            ("CY", 19m), ("CZ", 21m), ("DK", 25m), ("EE", 24m),
            ("FI", 25.5m), ("FR", 20m), ("DE", 19m), ("GR", 24m),
            ("HU", 27m), ("IE", 23m), ("IT", 22m), ("LV", 21m),
            ("LT", 21m), ("LU", 17m), ("MT", 18m), ("NL", 21m),
            ("PL", 23m), ("PT", 23m), ("RO", 21m), ("SK", 23m),
            ("SI", 22m), ("ES", 21m), ("SE", 25m)
        ];

        return rates
            .Select((entry, index) => new VatRate(
                -(index + 1),
                entry.Country,
                VatCategory.Standard,
                entry.Rate,
                EffectiveFrom))
            .ToArray();
    }
}
