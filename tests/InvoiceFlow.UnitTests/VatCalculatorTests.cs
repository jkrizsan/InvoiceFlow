using System.Globalization;
using InvoiceFlow.Application.Abstractions;
using InvoiceFlow.Application.Services;
using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.UnitTests;

public sealed class VatCalculatorTests
{
    private static readonly DateOnly EffectiveDate = new(2026, 9, 2);

    [Theory]
    [InlineData("FI", "25.5")]
    [InlineData("DE", "19")]
    [InlineData("HU", "27")]
    public async Task Consumer_sale_uses_destination_country_standard_rate(
        string countryCode,
        string expectedRate)
    {
        var sut = new VatCalculator(new StubVatRateRepository());

        decimal rate = await sut.GetRateAsync(
            "FI",
            countryCode,
            CustomerType.Consumer,
            null,
            VatCategory.Standard,
            EffectiveDate,
            CancellationToken.None);

        Assert.Equal(decimal.Parse(expectedRate, CultureInfo.InvariantCulture), rate);
    }

    [Fact]
    public async Task Eu_business_with_vat_number_uses_reverse_charge()
    {
        var sut = new VatCalculator(new StubVatRateRepository());

        decimal rate = await sut.GetRateAsync(
            "FI",
            "DE",
            CustomerType.Business,
            "DE123456789",
            VatCategory.Standard,
            EffectiveDate,
            CancellationToken.None);

        Assert.Equal(0m, rate);
        Assert.Equal(
            VatTreatment.ReverseCharge,
            sut.DetermineTreatment("FI", "DE", CustomerType.Business, "DE123456789"));
    }

    [Fact]
    public async Task Non_eu_customer_is_treated_as_export()
    {
        var sut = new VatCalculator(new StubVatRateRepository());

        decimal rate = await sut.GetRateAsync(
            "FI",
            "US",
            CustomerType.Business,
            null,
            VatCategory.Standard,
            EffectiveDate,
            CancellationToken.None);

        Assert.Equal(0m, rate);
        Assert.Equal(
            VatTreatment.Export,
            sut.DetermineTreatment("FI", "US", CustomerType.Business, null));
    }

    private sealed class StubVatRateRepository : IVatRateRepository
    {
        private static readonly Dictionary<string, decimal> Rates = new()
        {
            ["FI"] = 25.5m,
            ["DE"] = 19m,
            ["HU"] = 27m
        };

        public Task<decimal?> GetRateAsync(
            string countryCode,
            VatCategory category,
            DateOnly effectiveDate,
            CancellationToken cancellationToken) =>
            Task.FromResult(Rates.TryGetValue(countryCode, out decimal rate)
                ? (decimal?)rate
                : null);
    }
}
