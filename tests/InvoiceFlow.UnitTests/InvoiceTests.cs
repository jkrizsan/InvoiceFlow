using InvoiceFlow.Domain;
using InvoiceFlow.Domain.Entities;
using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.UnitTests;

public sealed class InvoiceTests
{
    [Fact]
    public void AddLine_calculates_and_rounds_totals_per_line()
    {
        Invoice invoice = CreateInvoice();

        invoice.AddLine("Consulting", 1.333m, 100m, VatCategory.Standard, 25.5m);

        Assert.Equal(133.30m, invoice.NetTotal);
        Assert.Equal(33.99m, invoice.VatTotal);
        Assert.Equal(167.29m, invoice.GrossTotal);
    }

    [Fact]
    public void Issued_invoice_cannot_be_edited()
    {
        Invoice invoice = CreateInvoice();
        invoice.AddLine("Consulting", 1m, 100m, VatCategory.Standard, 25.5m);
        invoice.Issue("INV-20260902-ABC12345", DateTimeOffset.UtcNow);

        DomainRuleException exception = Assert.Throws<DomainRuleException>(() =>
            invoice.AddLine("Another item", 1m, 10m, VatCategory.Standard, 25.5m));

        Assert.Contains("draft", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Paid_invoice_cannot_be_cancelled()
    {
        Invoice invoice = CreateInvoice();
        invoice.AddLine("Consulting", 1m, 100m, VatCategory.Standard, 25.5m);
        invoice.Issue("INV-20260902-ABC12345", DateTimeOffset.UtcNow);
        invoice.MarkPaid(DateTimeOffset.UtcNow);

        Assert.Throws<DomainRuleException>(() => invoice.Cancel(DateTimeOffset.UtcNow));
    }

    private static Invoice CreateInvoice() =>
        Invoice.Create(
            "test-owner",
            new DateOnly(2026, 9, 2),
            new DateOnly(2026, 9, 16),
            "EUR",
            "Seller Oy",
            "1234567-1",
            "FI12345671",
            "Seller Street 1, Finland",
            "FI",
            "Customer",
            "customer@example.com",
            "Customer Street 2, Finland",
            "FI",
            CustomerType.Consumer,
            null,
            VatTreatment.Domestic,
            null,
            DateTimeOffset.UtcNow);
}
