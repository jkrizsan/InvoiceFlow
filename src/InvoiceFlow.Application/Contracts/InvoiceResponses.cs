using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Application.Contracts;

public sealed record InvoiceLineResponse(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    VatCategory VatCategory,
    decimal VatRate,
    decimal NetAmount,
    decimal VatAmount,
    decimal GrossAmount);

public sealed record CustomerResponse(
    string Name,
    string Email,
    string Address,
    string CountryCode,
    CustomerType Type,
    string? VatNumber);

public sealed record SellerResponse(
    string Name,
    string BusinessId,
    string VatNumber,
    string Address,
    string CountryCode);

public sealed record InvoiceResponse(
    Guid Id,
    string? Number,
    InvoiceStatus Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    SellerResponse Seller,
    CustomerResponse Customer,
    VatTreatment VatTreatment,
    decimal NetTotal,
    decimal VatTotal,
    decimal GrossTotal,
    string? Notes,
    Guid Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<InvoiceLineResponse> Lines);

public sealed record InvoiceSummaryResponse(
    Guid Id,
    string? Number,
    InvoiceStatus Status,
    DateOnly IssueDate,
    DateOnly DueDate,
    string CustomerName,
    string CustomerCountryCode,
    string Currency,
    decimal GrossTotal,
    Guid Version);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => TotalCount == 0
        ? 0
        : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

