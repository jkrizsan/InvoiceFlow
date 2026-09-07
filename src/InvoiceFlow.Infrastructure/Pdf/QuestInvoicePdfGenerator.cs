using InvoiceFlow.Application.Abstractions;
using InvoiceFlow.Application.Contracts;
using InvoiceFlow.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvoiceFlow.Infrastructure.Pdf;

public sealed class QuestInvoicePdfGenerator : IInvoicePdfGenerator
{
    public byte[] Generate(InvoiceResponse invoice) =>
        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(style => style.FontSize(10));
                page.Header().Element(container => ComposeHeader(container, invoice));
                page.Content().Element(container => ComposeContent(container, invoice));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();

    private static void ComposeHeader(IContainer container, InvoiceResponse invoice)
    {
        container.PaddingBottom(20).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("INVOICE").FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                column.Item().Text(invoice.Number ?? "DRAFT").FontSize(12);
                column.Item().Text(invoice.Status.ToString()).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(column =>
            {
                column.Item().Text(invoice.Seller.Name).SemiBold();
                column.Item().Text(invoice.Seller.Address);
                column.Item().Text($"Business ID: {invoice.Seller.BusinessId}");
                column.Item().Text($"VAT: {invoice.Seller.VatNumber}");
            });
        });
    }

    private static void ComposeContent(IContainer container, InvoiceResponse invoice)
    {
        container.Column(column =>
        {
            column.Spacing(14);
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(details =>
                {
                    details.Item().Text("BILL TO").SemiBold().FontColor(Colors.Grey.Darken2);
                    details.Item().Text(invoice.Customer.Name).SemiBold();
                    details.Item().Text(invoice.Customer.Address);
                    details.Item().Text(invoice.Customer.Email);
                    if (!string.IsNullOrWhiteSpace(invoice.Customer.VatNumber))
                    {
                        details.Item().Text($"VAT: {invoice.Customer.VatNumber}");
                    }
                });

                row.ConstantItem(190).Column(details =>
                {
                    details.Item().Text($"Issue date: {invoice.IssueDate:yyyy-MM-dd}");
                    details.Item().Text($"Due date: {invoice.DueDate:yyyy-MM-dd}");
                    details.Item().Text($"Currency: {invoice.Currency}");
                    details.Item().Text($"VAT treatment: {invoice.VatTreatment}");
                });
            });

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1.4f);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1.4f);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Description");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Unit net");
                    header.Cell().Element(HeaderCell).AlignRight().Text("VAT");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Gross");
                });

                foreach (InvoiceLineResponse line in invoice.Lines)
                {
                    table.Cell().Element(BodyCell).Text(line.Description);
                    table.Cell().Element(BodyCell).AlignRight().Text(line.Quantity.ToString("0.###"));
                    table.Cell().Element(BodyCell).AlignRight().Text(Money(line.UnitPrice, invoice.Currency));
                    table.Cell().Element(BodyCell).AlignRight().Text($"{line.VatRate:0.##}%");
                    table.Cell().Element(BodyCell).AlignRight().Text(Money(line.GrossAmount, invoice.Currency));
                }
            });

            column.Item().AlignRight().Width(230).Column(totals =>
            {
                totals.Item().Row(row =>
                {
                    row.RelativeItem().Text("Net total");
                    row.RelativeItem().AlignRight().Text(Money(invoice.NetTotal, invoice.Currency));
                });
                totals.Item().Row(row =>
                {
                    row.RelativeItem().Text("VAT total");
                    row.RelativeItem().AlignRight().Text(Money(invoice.VatTotal, invoice.Currency));
                });
                totals.Item().PaddingTop(5).BorderTop(1).Row(row =>
                {
                    row.RelativeItem().Text("Total").SemiBold();
                    row.RelativeItem().AlignRight().Text(Money(invoice.GrossTotal, invoice.Currency)).SemiBold();
                });
            });

            if (invoice.VatTreatment == VatTreatment.ReverseCharge)
            {
                column.Item().Text("Reverse charge — VAT accounted for by the customer.").Italic();
            }

            if (!string.IsNullOrWhiteSpace(invoice.Notes))
            {
                column.Item().PaddingTop(10).Column(notes =>
                {
                    notes.Item().Text("Notes").SemiBold();
                    notes.Item().Text(invoice.Notes);
                });
            }
        });
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.Background(Colors.Blue.Medium).Padding(6).DefaultTextStyle(style =>
            style.FontColor(Colors.White).SemiBold());

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(6).PaddingHorizontal(4);

    private static string Money(decimal value, string currency) => $"{value:N2} {currency}";
}
