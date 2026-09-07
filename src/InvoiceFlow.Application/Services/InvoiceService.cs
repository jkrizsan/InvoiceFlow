using InvoiceFlow.Application.Abstractions;
using InvoiceFlow.Application.Contracts;
using InvoiceFlow.Application.Exceptions;
using InvoiceFlow.Domain;
using InvoiceFlow.Domain.Entities;
using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Application.Services;

public sealed class InvoiceService(
    IInvoiceRepository invoices,
    IVatCalculator vatCalculator,
    IInvoicePdfGenerator pdfGenerator,
    SellerProfile seller,
    TimeProvider timeProvider)
{
    public async Task<PagedResponse<InvoiceSummaryResponse>> SearchAsync(
        string ownerId,
        InvoiceQuery query,
        CancellationToken cancellationToken)
    {
        (IReadOnlyList<Invoice> items, int totalCount) = await invoices.SearchAsync(
            ownerId,
            query.Status,
            query.Customer,
            query.Page,
            query.PageSize,
            cancellationToken);

        return new PagedResponse<InvoiceSummaryResponse>(
            items.Select(MapSummary).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }

    public async Task<InvoiceResponse> GetAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken) =>
        Map(await GetInvoiceAsync(id, ownerId, cancellationToken));

    public async Task<InvoiceResponse> CreateAsync(
        string ownerId,
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        DateOnly issueDate = request.IssueDate ?? DateOnly.FromDateTime(now.UtcDateTime);
        DateOnly dueDate = request.DueDate ?? issueDate.AddDays(14);
        string currency = (request.Currency ?? seller.DefaultCurrency).ToUpperInvariant();

        VatTreatment treatment = vatCalculator.DetermineTreatment(
            seller.CountryCode,
            request.Customer.CountryCode,
            request.Customer.Type,
            request.Customer.VatNumber);

        var invoice = Invoice.Create(
            ownerId,
            issueDate,
            dueDate,
            currency,
            seller.Name,
            seller.BusinessId,
            seller.VatNumber,
            seller.Address,
            seller.CountryCode,
            request.Customer.Name,
            request.Customer.Email,
            request.Customer.Address,
            request.Customer.CountryCode,
            request.Customer.Type,
            request.Customer.VatNumber,
            treatment,
            request.Notes,
            now);

        await AddLinesAsync(invoice, request.Lines, cancellationToken);
        invoices.Add(invoice);
        await invoices.SaveChangesAsync(cancellationToken);
        return Map(invoice);
    }

    public async Task<InvoiceResponse> ReplaceAsync(
        Guid id,
        string ownerId,
        Guid? expectedVersion,
        ReplaceInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        Invoice invoice = await GetInvoiceAsync(id, ownerId, cancellationToken);
        VerifyVersion(invoice, expectedVersion);

        DateTimeOffset now = timeProvider.GetUtcNow();
        DateOnly issueDate = request.IssueDate ?? invoice.IssueDate;
        DateOnly dueDate = request.DueDate ?? invoice.DueDate;
        VatTreatment treatment = vatCalculator.DetermineTreatment(
            invoice.SellerCountryCode,
            request.Customer.CountryCode,
            request.Customer.Type,
            request.Customer.VatNumber);

        invoice.ReplaceDraft(
            issueDate,
            dueDate,
            request.Currency,
            request.Customer.Name,
            request.Customer.Email,
            request.Customer.Address,
            request.Customer.CountryCode,
            request.Customer.Type,
            request.Customer.VatNumber,
            treatment,
            request.Notes,
            now);

        await AddLinesAsync(invoice, request.Lines, cancellationToken);
        await invoices.SaveChangesAsync(cancellationToken);
        return Map(invoice);
    }

    public async Task<InvoiceResponse> PatchAsync(
        Guid id,
        string ownerId,
        Guid? expectedVersion,
        PatchInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        Invoice invoice = await GetInvoiceAsync(id, ownerId, cancellationToken);
        VerifyVersion(invoice, expectedVersion);
        DateTimeOffset now = timeProvider.GetUtcNow();

        bool hasDraftChanges = request.DueDate.HasValue || request.Notes is not null || request.ClearNotes;
        if (hasDraftChanges)
        {
            invoice.PatchDraft(request.DueDate, request.Notes, request.ClearNotes, now);
        }

        if (request.Status.HasValue && request.Status != invoice.Status)
        {
            ApplyStatus(invoice, request.Status.Value, now);
        }

        await invoices.SaveChangesAsync(cancellationToken);
        return Map(invoice);
    }

    public async Task DeleteAsync(
        Guid id,
        string ownerId,
        Guid? expectedVersion,
        CancellationToken cancellationToken)
    {
        Invoice invoice = await GetInvoiceAsync(id, ownerId, cancellationToken);
        VerifyVersion(invoice, expectedVersion);

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new DomainRuleException("Only draft invoices can be deleted; cancel an issued invoice instead.");
        }

        invoices.Remove(invoice);
        await invoices.SaveChangesAsync(cancellationToken);
    }

    public async Task<(byte[] Content, string FileName)> GetPdfAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken)
    {
        InvoiceResponse invoice = Map(await GetInvoiceAsync(id, ownerId, cancellationToken));
        string fileName = $"{invoice.Number ?? $"draft-{invoice.Id:N}"}.pdf";
        return (pdfGenerator.Generate(invoice), fileName);
    }

    private async Task AddLinesAsync(
        Invoice invoice,
        IEnumerable<InvoiceLineRequest> lines,
        CancellationToken cancellationToken)
    {
        foreach (InvoiceLineRequest line in lines)
        {
            decimal rate = await vatCalculator.GetRateAsync(
                invoice.SellerCountryCode,
                invoice.CustomerCountryCode,
                invoice.CustomerType,
                invoice.CustomerVatNumber,
                line.VatCategory,
                invoice.IssueDate,
                cancellationToken);

            invoice.AddLine(
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.VatCategory,
                rate);
        }
    }

    private async Task<Invoice> GetInvoiceAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken) =>
        await invoices.GetAsync(id, ownerId, cancellationToken)
        ?? throw new NotFoundException($"Invoice '{id}' was not found.");

    private static void VerifyVersion(Invoice invoice, Guid? expectedVersion)
    {
        if (!expectedVersion.HasValue)
        {
            throw new PreconditionRequiredException(
                "Supply the latest invoice ETag in the If-Match header.");
        }

        if (invoice.Version != expectedVersion.Value)
        {
            throw new PreconditionFailedException(
                "The invoice changed after it was read. Reload it and retry with the new ETag.");
        }
    }

    private static void ApplyStatus(Invoice invoice, InvoiceStatus target, DateTimeOffset now)
    {
        switch (target)
        {
            case InvoiceStatus.Issued when invoice.Status == InvoiceStatus.Draft:
                invoice.Issue(GenerateInvoiceNumber(invoice.IssueDate), now);
                break;
            case InvoiceStatus.Paid when invoice.Status == InvoiceStatus.Issued:
                invoice.MarkPaid(now);
                break;
            case InvoiceStatus.Cancelled when invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Issued:
                invoice.Cancel(now);
                break;
            default:
                throw new DomainRuleException(
                    $"Invoice status cannot change from {invoice.Status} to {target}.");
        }
    }

    private static string GenerateInvoiceNumber(DateOnly issueDate) =>
        $"INV-{issueDate:yyyyMMdd}-{Guid.NewGuid():N}"[..21].ToUpperInvariant();

    private static InvoiceSummaryResponse MapSummary(Invoice invoice) =>
        new(
            invoice.Id,
            invoice.Number,
            invoice.Status,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.CustomerName,
            invoice.CustomerCountryCode,
            invoice.Currency,
            invoice.GrossTotal,
            invoice.Version);

    private static InvoiceResponse Map(Invoice invoice) =>
        new(
            invoice.Id,
            invoice.Number,
            invoice.Status,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.Currency,
            new SellerResponse(
                invoice.SellerName,
                invoice.SellerBusinessId,
                invoice.SellerVatNumber,
                invoice.SellerAddress,
                invoice.SellerCountryCode),
            new CustomerResponse(
                invoice.CustomerName,
                invoice.CustomerEmail,
                invoice.CustomerAddress,
                invoice.CustomerCountryCode,
                invoice.CustomerType,
                invoice.CustomerVatNumber),
            invoice.VatTreatment,
            invoice.NetTotal,
            invoice.VatTotal,
            invoice.GrossTotal,
            invoice.Notes,
            invoice.Version,
            invoice.CreatedAt,
            invoice.UpdatedAt,
            invoice.Lines.Select(line => new InvoiceLineResponse(
                line.Id,
                line.Description,
                line.Quantity,
                line.UnitPrice,
                line.VatCategory,
                line.VatRate,
                line.NetAmount,
                line.VatAmount,
                line.GrossAmount)).ToList());
}
