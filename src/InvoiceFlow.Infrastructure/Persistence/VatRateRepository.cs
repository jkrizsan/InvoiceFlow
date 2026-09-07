using InvoiceFlow.Application.Abstractions;
using InvoiceFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence;

public sealed class VatRateRepository(InvoiceDbContext dbContext) : IVatRateRepository
{
    public Task<decimal?> GetRateAsync(
        string countryCode,
        VatCategory category,
        DateOnly effectiveDate,
        CancellationToken cancellationToken) =>
        dbContext.VatRates
            .AsNoTracking()
            .Where(rate =>
                rate.CountryCode == countryCode &&
                rate.Category == category &&
                rate.ValidFrom <= effectiveDate &&
                (rate.ValidTo == null || rate.ValidTo >= effectiveDate))
            .OrderByDescending(rate => rate.ValidFrom)
            .Select(rate => (decimal?)rate.Rate)
            .FirstOrDefaultAsync(cancellationToken);
}

