using InvoiceFlow.Application.Abstractions;
using InvoiceFlow.Application.Exceptions;
using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Application.Services;

public sealed class VatCalculator(IVatRateRepository vatRates) : IVatCalculator
{
    private static readonly HashSet<string> EuCountries =
    [
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI",
        "FR", "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU",
        "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE"
    ];

    public VatTreatment DetermineTreatment(
        string sellerCountryCode,
        string customerCountryCode,
        CustomerType customerType,
        string? customerVatNumber)
    {
        string seller = Normalize(sellerCountryCode);
        string customer = Normalize(customerCountryCode);

        if (!EuCountries.Contains(customer))
        {
            return VatTreatment.Export;
        }

        if (seller != customer &&
            customerType == CustomerType.Business &&
            !string.IsNullOrWhiteSpace(customerVatNumber))
        {
            return VatTreatment.ReverseCharge;
        }

        return seller == customer
            ? VatTreatment.Domestic
            : VatTreatment.EuDestination;
    }

    public async Task<decimal> GetRateAsync(
        string sellerCountryCode,
        string customerCountryCode,
        CustomerType customerType,
        string? customerVatNumber,
        VatCategory category,
        DateOnly effectiveDate,
        CancellationToken cancellationToken)
    {
        VatTreatment treatment = DetermineTreatment(
            sellerCountryCode,
            customerCountryCode,
            customerType,
            customerVatNumber);

        if (category == VatCategory.Zero ||
            treatment is VatTreatment.ReverseCharge or VatTreatment.Export)
        {
            return 0m;
        }

        string country = Normalize(customerCountryCode);
        decimal? rate = await vatRates.GetRateAsync(
            country,
            category,
            effectiveDate,
            cancellationToken);

        return rate ?? throw new VatRateNotFoundException(
            $"No {category} VAT rate is configured for {country} on {effectiveDate:yyyy-MM-dd}.");
    }

    private static string Normalize(string countryCode) =>
        countryCode.Trim().ToUpperInvariant();
}

