using InvoiceFlow.Application.Abstractions;
using InvoiceFlow.Application.Exceptions;
using InvoiceFlow.Domain.Entities;
using InvoiceFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InvoiceFlow.Infrastructure.Persistence;

public sealed class InvoiceRepository(InvoiceDbContext dbContext) : IInvoiceRepository
{
    public Task<Invoice?> GetAsync(
        Guid id,
        string ownerId,
        CancellationToken cancellationToken) =>
        dbContext.Invoices
            .Include(invoice => invoice.Lines)
            .SingleOrDefaultAsync(
                invoice => invoice.Id == id && invoice.OwnerId == ownerId,
                cancellationToken);

    public async Task<(IReadOnlyList<Invoice> Items, int TotalCount)> SearchAsync(
        string ownerId,
        InvoiceStatus? status,
        string? customer,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Invoice> query = dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.OwnerId == ownerId);

        if (status.HasValue)
        {
            query = query.Where(invoice => invoice.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(customer))
        {
            string pattern = $"%{customer.Trim()}%";
            query = query.Where(invoice => EF.Functions.ILike(invoice.CustomerName, pattern));
        }

        int totalCount = await query.CountAsync(cancellationToken);
        List<Invoice> items = await query
            .OrderByDescending(invoice => invoice.IssueDate)
            .ThenByDescending(invoice => invoice.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Invoice invoice) => dbContext.Invoices.Add(invoice);

    public void Remove(Invoice invoice) => dbContext.Invoices.Remove(invoice);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(
                "The invoice was modified by another request.",
                exception);
        }
    }
}

