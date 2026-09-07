using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Application.Abstractions;

public interface IVatCalculator
{
    VatTreatment DetermineTreatment(
        string sellerCountryCode,
        string customerCountryCode,
        CustomerType customerType,
        string? customerVatNumber);

    Task<decimal> GetRateAsync(
        string sellerCountryCode,
        string customerCountryCode,
        CustomerType customerType,
        string? customerVatNumber,
        VatCategory category,
        DateOnly effectiveDate,
        CancellationToken cancellationToken);
}

