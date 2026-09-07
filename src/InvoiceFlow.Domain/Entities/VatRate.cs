using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Domain.Entities;

public sealed class VatRate
{
    private VatRate()
    {
    }

    public VatRate(
        int id,
        string countryCode,
        VatCategory category,
        decimal rate,
        DateOnly validFrom,
        DateOnly? validTo = null)
    {
        Id = id;
        CountryCode = countryCode.Trim().ToUpperInvariant();
        Category = category;
        Rate = rate;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public int Id { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public VatCategory Category { get; private set; }
    public decimal Rate { get; private set; }
    public DateOnly ValidFrom { get; private set; }
    public DateOnly? ValidTo { get; private set; }
}

