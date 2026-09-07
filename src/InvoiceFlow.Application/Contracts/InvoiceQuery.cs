using System.ComponentModel.DataAnnotations;
using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Application.Contracts;

public sealed class InvoiceQuery
{
    [EnumDataType(typeof(InvoiceStatus))]
    public InvoiceStatus? Status { get; init; }

    [StringLength(160)]
    public string? Customer { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}
