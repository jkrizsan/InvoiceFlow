using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Application.Abstractions;

public interface IVatRateRepository
{
    Task<decimal?> GetRateAsync(
        string countryCode,
        VatCategory category,
        DateOnly effectiveDate,
        CancellationToken cancellationToken);
}

