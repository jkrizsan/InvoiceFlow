using InvoiceFlow.Domain.Entities;
using InvoiceFlow.Domain.Enums;

namespace InvoiceFlow.Application.Abstractions;

public interface IInvoiceRepository
{
    Task<Invoice?> GetAsync(Guid id, string ownerId, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Invoice> Items, int TotalCount)> SearchAsync(
        string ownerId,
        InvoiceStatus? status,
        string? customer,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    void Add(Invoice invoice);
    void Remove(Invoice invoice);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

